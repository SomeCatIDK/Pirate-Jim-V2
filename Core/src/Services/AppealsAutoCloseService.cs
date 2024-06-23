using System;
using System.Threading.Tasks;
using Discord;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

public class AppealsAutoCloseService : IService
{
    public const string LockedMessage = "**This post is now locked.**\n\nWe automatically lock every appeal after five days.";

    public AppealsAutoCloseService(PirateJim bot)
    {
        
    }

    public async Task InitializeAsync(PirateJim bot)
    {
        var channel = await bot.DiscordClient.GetChannelAsync(UOChannels.Appeals);

        if (channel is not IForumChannel forum)
            return;


        foreach (var post in await forum.GetActiveThreadsAsync())
        {
            if (post.IsLocked) 
                continue;

            if ((DateTime.Now - post.CreatedAt.DateTime).TotalDays >= 4.9)
            {
                await post.SendMessageAsync(LockedMessage);
                await post.ModifyAsync((prop) => prop.Locked = true);
            }
        }
    }
}

