using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Modules;

// ReSharper disable twice StringLiteralTypo
[Group("slowmode", "Controls the slowmode properties of a channel.")]
public class UserTimeoutCommandModule : InteractionModuleBase
{
    [RequireOwner(Group = "ManageChannels")]
    [RequireUserPermission(ChannelPermission.ManageChannels, Group = "ManageChannels")]
    [SlashCommand("add", "Adds a timeout to a channel in seconds.")]
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
                await RespondAsync($"The specified channel's timeout is already set at {timeout} seconds.");
            else
            {
                timeoutChannel.Time = timeout;
                await RespondAsync($"The channel's timeout has been changed to {timeout} seconds.");
            }
        }

        await db.SaveChangesAsync();
    }

    [RequireOwner(Group = "ManageChannels")]
    [RequireUserPermission(ChannelPermission.ManageChannels, Group = "ManageChannels")]
    [SlashCommand("remove", "Removes a timeout to a channel.")]
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
    
    [RequireOwner(Group = "ManageMessages")]
    [RequireUserPermission(ChannelPermission.ManageMessages, Group = "ManageMessages")]
    [SlashCommand("reset", "Removes a timeout to a channel.")]
    public async Task ResetUserTimeout([ChannelTypes(ChannelType.Text)] IChannel channel, IUser user)
    {
        await using var db = new PirateJimDbContext();

        var userTimeout = db.UserTimeouts
            .FirstOrDefault(x => x.ChannelId == channel.Id && x.UserId == user.Id);

        if (userTimeout == null)
        {
            await RespondAsync("The specified user does not have a timeout in that channel.");
            return;
        }

        db.UserTimeouts.Remove(userTimeout);
        
        await RespondAsync("The specified user no longer has a timeout in that channel.");
        await db.SaveChangesAsync();
    }
}