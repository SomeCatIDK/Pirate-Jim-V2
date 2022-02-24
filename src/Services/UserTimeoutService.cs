using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

public class UserTimeoutService : IService
{
    public UserTimeoutService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;
    }

    private static async Task OnMessage(SocketMessage message)
    {
        if (message.Source != MessageSource.User)
            return;

        await using var db = new PirateJimDbContext();
        
        var timeoutChannel = db.GuildTimeoutChannels
            .FirstOrDefault(x => x.ChannelId == message.Channel.Id);

        if (timeoutChannel != null)
        {
            var timeoutUser = db.UserTimeouts
                .FirstOrDefault(x => x.ChannelId == message.Channel.Id && x.UserId == message.Author.Id);

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
                if ((DateTime.UtcNow - timeoutUser.TimeStamp).TotalSeconds >= timeoutChannel.Time)
                    timeoutUser.TimeStamp = DateTime.UtcNow;
                else
                    await message.DeleteAsync();
            }
        }

        await db.SaveChangesAsync();
    }
}