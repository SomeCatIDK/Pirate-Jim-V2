using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Modules;

public class UserTimeoutCommandModule : InteractionModuleBase
{
    [SlashCommand("paddchanneltimeout", "Adds a timeout to a channel in seconds.")]
    public async Task AddChannelTimeout([ChannelTypes(ChannelType.Text)] IChannel channel, int timeout)
    {
        if (timeout == 0)
        {
            await RespondAsync("Channel timeout cannot be less than or equal to zero.");
            return;
        }
        
        await using var db = new PirateJimDbContext();

        var timeoutChannel = db.GuildTimeoutChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (timeoutChannel == null)
        {
            timeoutChannel = new GuildTimeoutChannel
            {
                ChannelId = channel.Id,
                Time = timeout
            };

            await db.GuildTimeoutChannels.AddAsync(timeoutChannel);
            await RespondAsync($"A timeout of {timeout} seconds has been added to the channel.");
        }
        else
        {
            if (timeoutChannel.Time == timeout)
            {
                await RespondAsync($"The specified channel's timeout is already set at {timeout} seconds.");
            }
            else
            {
                timeoutChannel.Time = timeout;
                await RespondAsync($"The channel's timeout has been changed to {timeout} seconds.");
            }
        }

        await db.SaveChangesAsync();
    }

    [SlashCommand("premovechanneltimeout", "Removes a timeout from a channel.")]
    public async Task RemoveChannelTimeout([ChannelTypes(ChannelType.Text)] IChannel channel)
    {
        await using var db = new PirateJimDbContext();

        var timeoutChannel = db.GuildTimeoutChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (timeoutChannel == null)
        {
            await RespondAsync("The specified channel does not have a timeout.");
            return;
        }

        db.GuildTimeoutChannels.Remove(timeoutChannel);

        await RespondAsync("The specified channel no longer has a timeout.");
        await db.SaveChangesAsync();
    }
}