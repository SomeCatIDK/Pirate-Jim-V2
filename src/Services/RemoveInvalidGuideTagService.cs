using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

public class RemoveInvalidGuideTagService : IService, IInitializableService
{
    private const string GuideTag = "Guide";

    private readonly Dictionary<ulong, ulong> _guideTagIds = new();

    private readonly PirateJim _bot;

    private ulong? _lastAuditLog;

    public RemoveInvalidGuideTagService(PirateJim bot)
    {
        _bot = bot;
    }

    public async Task InitializeAsync()
    {
        await RegisterGuideTagAsync(UOChannels.ServerHostingForum);
        await RegisterGuideTagAsync(UOChannels.UnturnedSupportForum);
        await RegisterGuideTagAsync(UOChannels.ModdingSupportForum);

        _bot.DiscordClient.ThreadCreated += OnThreadCreated;
        _bot.DiscordClient.ThreadUpdated += OnThreadUpdated;
    }

    private async Task RegisterGuideTagAsync(ulong channelId)
    {
        var channel = await _bot.DiscordClient.GetChannelAsync(channelId);

        if (channel is not IForumChannel forum)
            return;

        var tags = forum.Tags.ToList();
        var tagIndex = tags.FindIndex(t => t.Name == GuideTag);

        if (tagIndex == -1)
            return;

        _guideTagIds.Add(channelId, tags[tagIndex].Id);
    }

    private async Task OnThreadCreated(SocketThreadChannel post)
    {
        await RemoveInvalidGuideTagAsync(post, post.Owner?.GuildUser);
    }

    private async Task OnThreadUpdated(Cacheable<SocketThreadChannel, ulong> cachedPost, SocketThreadChannel updatedPost)
    {
        await RemoveInvalidGuideTagAsync(updatedPost, await GetModeratorThreadUpdated(updatedPost) ?? updatedPost.Owner?.GuildUser);
    }

    private async Task<IGuildUser?> GetModeratorThreadUpdated(SocketThreadChannel updatedPost)
    {
        await foreach (var logs in updatedPost.Guild.GetAuditLogsAsync(4, actionType: ActionType.ThreadUpdate))
        {
            if (logs == null)
                continue;

            RestAuditLogEntry? logEntry = logs.FirstOrDefault(log =>
            {
                if (_lastAuditLog != null && log.Id <= _lastAuditLog) // AuditLog too old / Already consumed
                    return false;

                if (log.CreatedAt.DateTime.AddSeconds(30) < DateTime.Now) // AuditLog too old
                    return false;

                if (log.Data is not ThreadUpdateAuditLogData logData)
                    return false;

                if (logData.Thread == null) // Thread has been Deleted
                    return false;

                return logData.Thread.Id == updatedPost.Id;
            });

            if (logEntry == null)
                continue;

            _lastAuditLog = logEntry.Id;

            return await ((IGuild)updatedPost.Guild).GetUserAsync(logEntry.User.Id);
        }

        return null;
    }

    private async Task RemoveInvalidGuideTagAsync(SocketThreadChannel post, IGuildUser? guildUser)
    {
        if (post.AppliedTags == null)
            return;

        if (!_guideTagIds.TryGetValue(post.ParentChannel.Id, out var guideTagId))
            return;
        
        if (!post.AppliedTags.Contains(guideTagId))
            return;

        guildUser ??= await ((IGuild) post.Guild).GetUserAsync(((IThreadChannel) post).OwnerId);

        if (guildUser == null)
        {
            Console.WriteLine($"GuildUser is null. Post name: {post.Name} | Channel name: {post.ParentChannel.Name}");
            return;
        }

        if (guildUser.GuildPermissions.ManageThreads)
            return;

        var newTags = post.AppliedTags.ToList();
        newTags.Remove(guideTagId);
        
        await post.ModifyAsync(postProperties => postProperties.AppliedTags = newTags);
    }
}