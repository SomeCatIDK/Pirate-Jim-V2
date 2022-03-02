using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

// This service listens for user messages and manages user timeouts in timeout channels.
public class UserTimeoutService : IService
{
    public UserTimeoutService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;
    }

    private static async Task OnMessage(SocketMessage message)
    {
        // Message was not sent by a user.
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser author)
            return;

        // Author has ManageMessages, ignore checking for timeout.
        if (author.GuildPermissions.ManageMessages)
            return;

        await using var db = new PirateJimDbContext();
        
        var timeoutChannel = db.GuildTimeoutChannels
            .FirstOrDefault(x => x.ChannelId == message.Channel.Id);

        // Channel is not a timeout channel, no need to continue.
        if (timeoutChannel == null)
            return;
        
        var timeoutUser = db.UserTimeouts
            .FirstOrDefault(x => x.ChannelId == message.Channel.Id && x.UserId == message.Author.Id);

        // User has no previous timeout.
        if (timeoutUser == null)
        {
            timeoutUser = new UserTimeout
            {
                ChannelId = message.Channel.Id,
                UserId = message.Author.Id,
                TimeStamp = DateTime.UtcNow
            };

            db.UserTimeouts.Add(timeoutUser);
        }
        else
        {
            // User previously had a timeout but it has since expired.
            if ((DateTime.UtcNow - timeoutUser.TimeStamp).TotalSeconds >= timeoutChannel.Time)
                timeoutUser.TimeStamp = DateTime.UtcNow;
            
            // User has an active timeout, delete the message.
            else
                await message.DeleteAsync();
        }
            
        await db.SaveChangesAsync();
    }
}