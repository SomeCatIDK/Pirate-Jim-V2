using Cottle;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;
using SomeCatIDK.PirateJim.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.Services;

public class SolvedPostsService : IService, IInitializableService
{
    private const string SolvedTag = "Solved";
    
    private const string SolvedDefaultDescription = "When your question is answered either use `/solved` or the button below to mark the question as resolved.\n\nRemember to ask __specific questions__ and provide __relevant details__.";
    private const string SolvedButtonId = "MarkAsSolvedButton";
    
    private const string ReopenDefaultDescription = "Thank you for marking this question as solved. You're welcome to make a new post if you have another question!\n\n*This post is now set to auto-hide after one hour of inactivity.*";
    private const string ReopenButtonId = "ReopenSolvedButton";

    private const string StaleDefaultDescription = "This question is considered stale and has been closed.\nIf your question was answered do `/solved`.\nIf your question was **not** answered feel free to bump it or repost.";

    public record SolvedForumChannel(ulong ChannelId, string? SolvedDescription = null, string? ReopenDescription = null, string? StaleDescription = null, params string[]? IgnoreWithTags);
    public record RegisteredSolvedChannel(SolvedForumChannel SolvedChannel, ulong SolvedTag, ulong[]? IgnoreWithTags);

    private readonly Dictionary<ulong, RegisteredSolvedChannel> _registeredSolvedChannels = [];

    /// Maximum Value is ~24 Days
    /// "https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.delay?view=net-10.0#system-threading-tasks-task-delay(system-timespan)"
    private readonly TimeSpan MarkPostAsInactiveDuration = TimeSpan.FromDays(2.5);

    private readonly PirateJim _bot;

    public SolvedPostsService(PirateJim bot)
    {
        _bot = bot;
    }

    public async Task InitializeAsync()
    {
        // Jdance wanted each channel to be able to have their own SolvedDescription
        var serverHosting = new SolvedForumChannel(UOChannels.ServerHostingForum);
        var unturnedSupport = new SolvedForumChannel(UOChannels.UnturnedSupportForum, 
            SolvedDescription: "When your question is answered either use `/solved` or the button below to mark the question as resolved.\n\n" +
            "Remember to ask __specific questions__ and provide __relevant details__. You can find tips on how to ask a good question [here](https://discord.com/channels/324229387295653889/1477781539994468392/1477784244360319037).",
            IgnoreWithTags: [ "Guide", "Common Issues" ]);
        var moddingSupport = new SolvedForumChannel(UOChannels.ModdingSupportForum, IgnoreWithTags: "Guide");

        await RegisterSolvedChannelAsync(serverHosting);
        await RegisterSolvedChannelAsync(unturnedSupport);
        await RegisterSolvedChannelAsync(moddingSupport);

        _bot.DiscordClient.ThreadCreated += OnUnsolvedSupportPostCreated;
        _bot.DiscordClient.ButtonExecuted += OnMarkAsSolvedButton;
        _bot.DiscordClient.ButtonExecuted += OnReopenButton;
        _bot.DiscordClient.MessageReceived += OnDeleteInactivePostNotification;
    }

    private async Task RegisterSolvedChannelAsync(SolvedForumChannel solvedChannel)
    {
        var channel = await _bot.DiscordClient.GetChannelAsync(solvedChannel.ChannelId);

        if (channel is not IForumChannel forum) return;

        var tags = forum.Tags.ToList();
        var tagIndex = tags.FindIndex(t => t.Name.Contains(SolvedTag, StringComparison.InvariantCultureIgnoreCase));

        if (tagIndex == -1) return;

        var ignoreWithTags = Array.Empty<ulong>();
        
        if (solvedChannel.IgnoreWithTags != null && solvedChannel.IgnoreWithTags.Length > 0)
            ignoreWithTags = tags.Where(t => solvedChannel.IgnoreWithTags.Contains(t.Name, StringComparer.InvariantCultureIgnoreCase)).Select(t => t.Id).ToArray();

        var registeredChannel = new RegisteredSolvedChannel(solvedChannel, tags[tagIndex].Id, ignoreWithTags);
        _registeredSolvedChannels[solvedChannel.ChannelId] = registeredChannel;

        // Run on a seperate thread
        _ = Task.Run(() => NotifyInactivePosts(registeredChannel));
    }

    private async Task NotifyInactivePosts(RegisteredSolvedChannel solvedChannel)
    {
        var channel = await _bot.DiscordClient.GetChannelAsync(solvedChannel.SolvedChannel.ChannelId);
        if (channel is not IForumChannel forum) return;

        while (true)
        {
            var activeThreads = await forum.GetActiveThreadsAsync();

            foreach (var activeThread in activeThreads)
            {
                if (activeThread.Flags.HasFlag(ChannelFlags.Pinned)) continue;
                if (activeThread.IsLocked || activeThread.IsArchived) continue;
                if (activeThread.AppliedTags.Any(t => t == solvedChannel.SolvedTag || solvedChannel.IgnoreWithTags.Contains(t))) continue;
                if ((DateTimeOffset.Now - activeThread.ArchiveTimestamp) <= MarkPostAsInactiveDuration) continue;

                var embedMessage = new EmbedBuilder().WithDescription(solvedChannel.SolvedChannel.StaleDescription ?? StaleDefaultDescription).WithColor(7506394).Build();
                await activeThread.SendMessageAsync(embed: embedMessage);
                await activeThread.ModifyAsync(postProperties => postProperties.Archived = true);
            }
            await Task.Delay(MarkPostAsInactiveDuration);
        }
    }

    private async Task OnDeleteInactivePostNotification(SocketMessage message) => await DeleteInactivePostNotification(message.Channel);
    private async Task DeleteInactivePostNotification(IChannel channel)
    {
        if (channel is not SocketThreadChannel thread || thread.ParentChannel is not SocketForumChannel forumChannel) return;
        if (thread.IsLocked) return;
        if (!_registeredSolvedChannels.TryGetValue(forumChannel.Id, out var solvedChannel)) return;
        if (thread.AppliedTags.Any(t => t == solvedChannel.SolvedTag || solvedChannel.IgnoreWithTags.Contains(t))) return;

        var messages = await thread.GetMessagesAsync(4).FlattenAsync();
        var inactivePostMessage = messages.FirstOrDefault(m => m.Author.Id == _bot.DiscordClient.CurrentUser.Id && m.Components.Count == 0);
        if (inactivePostMessage == null) return;

        await thread.DeleteMessageAsync(inactivePostMessage);
    }

    private async Task OnUnsolvedSupportPostCreated(SocketThreadChannel thread)
    {
        if (thread.HasJoined) return; // SendMessageAsync() makes the bot join the channel which triggers this event again
        if (thread.ParentChannel is not SocketForumChannel forumChannel) return;
        if (!_registeredSolvedChannels.TryGetValue(forumChannel.Id, out var solvedChannel)) return;
        if (thread.AppliedTags.Any(t => solvedChannel.IgnoreWithTags.Contains(t))) return;
        
        var embedMessage = new EmbedBuilder().WithDescription(solvedChannel.SolvedChannel.SolvedDescription ?? SolvedDefaultDescription).WithColor(7506394).Build();
        var button = new ComponentBuilder().WithButton("Mark as Solved", SolvedButtonId, ButtonStyle.Success, Emoji.Parse(":white_check_mark:")).Build();

        await thread.SendMessageAsync(embed: embedMessage, components: button);
    }

    private async Task OnMarkAsSolvedButton(SocketMessageComponent component)
    {
        if (component.Data.CustomId != SolvedButtonId) return;
        await MarkAsSolvedAsync(component.Message, component.Channel, component.User, response =>
        {
            if (string.IsNullOrEmpty(response)) return component.DeferAsync(true);
            return component.RespondAsync(response, flags: MessageFlags.Ephemeral);
        });
    }

    public async Task MarkAsSolvedAsync(IMessage? message, IChannel channel, IUser user, Func<string?, Task> respondAsync)
    {
        if (channel is not SocketThreadChannel thread || thread.ParentChannel is not SocketForumChannel forumChannel)
        {
            await respondAsync("This is not a forum post.");
            return;
        }
        if (thread.IsLocked)
        {
            await respondAsync("Unable to mark locked thread as solved.");
            return;
        }
        if (thread.Owner.Id != user.Id && (user is not IGuildUser guildUser || !guildUser.GuildPermissions.ManageThreads))
        {
            await respondAsync("You are not allowed to mark this post as solved!");
            return;
        }

        if (!_registeredSolvedChannels.TryGetValue(forumChannel.Id, out var solvedChannel))
        {
            await respondAsync("This post can't be marked as solved.");
            return;
        }

        if (thread.AppliedTags.Contains(solvedChannel.SolvedTag))
        {
            await respondAsync("This post is already solved.");
            return;
        }

        if (message == null)
        {
            var messages = await thread.GetMessagesAsync().FlattenAsync();
            message = messages.LastOrDefault(m => m.Author.Id == _bot.DiscordClient.CurrentUser.Id);
        }

        await DeleteInactivePostNotification(channel);

        var newTags = thread.AppliedTags.ToList();
        newTags.Add(solvedChannel.SolvedTag);

        await thread.ModifyAsync(postProperties =>
        {
            postProperties.AppliedTags = newTags;
            postProperties.AutoArchiveDuration = ThreadArchiveDuration.OneHour;
        });

        var embedMessage = new EmbedBuilder().WithDescription(solvedChannel.SolvedChannel.ReopenDescription ?? ReopenDefaultDescription).WithColor(7506394).Build();
        var button = new ComponentBuilder().WithButton("Reopen", ReopenButtonId, ButtonStyle.Primary, Emoji.Parse(":arrows_counterclockwise:")).Build();

        await thread.SendMessageAsync(embed: embedMessage, messageReference: message != null ? new MessageReference(message.Id, failIfNotExists: false) : null, components: button);
        await respondAsync(null);
    }

    private async Task OnReopenButton(SocketMessageComponent component)
    {
        if (component.Data.CustomId != ReopenButtonId) return;
        if (component.Channel is not SocketThreadChannel thread || thread.ParentChannel is not SocketForumChannel forumChannel) return;
        if (thread.IsLocked)
        {
            await component.RespondAsync("Unable to reopen locked post.", flags: MessageFlags.Ephemeral);
            return;
        }
        if (thread.Owner.Id != component.User.Id && (component.User is not SocketGuildUser guildUser || !guildUser.GuildPermissions.ManageThreads))
        {
            await component.RespondAsync("You are not allowed to reopen this post!", flags: MessageFlags.Ephemeral);
            return;
        }

        if (!_registeredSolvedChannels.TryGetValue(forumChannel.Id, out var solvedChannel)) return;

        if (!thread.AppliedTags.Contains(solvedChannel.SolvedTag))
        {
            await component.RespondAsync("This post hasn't been solved.", flags: MessageFlags.Ephemeral);
            return;
        }

        var newTags = thread.AppliedTags.ToList();
        newTags.Remove(solvedChannel.SolvedTag);

        await thread.ModifyAsync(postProperties =>
        {
            postProperties.AppliedTags = newTags;
            postProperties.AutoArchiveDuration = ThreadArchiveDuration.OneWeek;
        });

        await thread.DeleteMessageAsync(component.Message);
        await component.DeferAsync(true);
    }
}
