using System;
using System.Threading.Tasks;
using Discord;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

public class AppealsAutoCloseService : IService, IInitializableService
{
    // ReSharper disable once MemberCanBePrivate.Global
    public const string LockedMessage = "**This post is now locked.**\n\nWe automatically lock every appeal after five days.";

    private readonly PirateJim _bot;

    public AppealsAutoCloseService(PirateJim bot)
    {
        _bot = bot;
    }

    public async Task InitializeAsync()
    {
        var channel = await _bot.DiscordClient.GetChannelAsync(UOChannels.AppealsForum);

        if (channel is not IForumChannel forum)
            return;
        
        foreach (var post in await forum.GetActiveThreadsAsync())
        {
            if (post.IsLocked) 
                continue;

            if (!((DateTime.Now - post.CreatedAt.DateTime).TotalDays >= 4.9))
                continue;
            
            await post.SendMessageAsync(LockedMessage);
            await post.ModifyAsync((prop) => prop.Locked = true);
        }
    }
}