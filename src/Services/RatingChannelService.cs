using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

// This service listens for user messages and adds the rating reactions.
public class RatingChannelService : IService
{
    private readonly Emoji[] _ratingEmojis = new Emoji[3];
    
    public RatingChannelService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;

        _ratingEmojis[0] = Emoji.Parse(":thumbsup:");
        _ratingEmojis[1] = Emoji.Parse(":thumbsdown:");
        _ratingEmojis[2] = Emoji.Parse(":heart:");
    }

    private async Task OnMessage(SocketMessage message)
    {
        // Message was not sent by a user.
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser)
            return;

        await using var db = new PirateJimDbContext();
        
        var ratingChannel = db.GuildRatingChannels
            .FirstOrDefault(x => x.ChannelId == message.Channel.Id);

        // Channel is not a rating channel, no need to continue.
        if (ratingChannel == null)
            return;

        foreach (var emoji in _ratingEmojis)
            await message.AddReactionAsync(emoji);

        await Task.CompletedTask;
    }
}