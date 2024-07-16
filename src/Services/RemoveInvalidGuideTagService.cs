using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

public class RemoveInvalidGuideTagService : IService
{
    private const string GuideTag = "Guide";

    private Dictionary<ulong, ulong> _guideTagIds = new Dictionary<ulong, ulong>();

    private readonly PirateJim _bot;

    public RemoveInvalidGuideTagService(PirateJim bot)
    {
        _bot = bot;
    }

    public async Task InitializeAsync()
    {
        await RegisterGuideTagAsync(UOChannels.ServerHosting);
        await RegisterGuideTagAsync(UOChannels.UnturnedSupport);
        await RegisterGuideTagAsync(UOChannels.ModdingSupport);

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
        await RemoveInvalidGuideTagAsync(post);
    }

    private async Task OnThreadUpdated(Cacheable<SocketThreadChannel, ulong> cachedPost, SocketThreadChannel updatedPost)
    {
        await RemoveInvalidGuideTagAsync(updatedPost);
    }

    private async Task RemoveInvalidGuideTagAsync(SocketThreadChannel post)
    {
        if (!_guideTagIds.TryGetValue(post.ParentChannel.Id, out ulong guideTagId))
            return;

        if (!post.AppliedTags.Contains(guideTagId))
            return;

        if (post.Owner.GuildUser.GuildPermissions.ManageThreads)
            return;

        var newTags = post.AppliedTags.ToList();
        newTags.Remove(guideTagId);
        await post.ModifyAsync((post) => post.AppliedTags = newTags);
    }
}