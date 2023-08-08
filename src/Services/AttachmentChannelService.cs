using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

// This service listens for user messages deletes any that don't have exactly 1 embeddable attachment
public class AttachmentChannelService : IService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4", ".gif", ".gifv", ".mp3", ".wav", ".ogg", ".mov" };
        
    public AttachmentChannelService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;
    }

    private async Task OnMessage(SocketMessage message)
    {
        // Message was not sent by a user.
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser author)
            return;

        // Author has ManageMessages, ignore checking for attachments.
        if (author.GuildPermissions.ManageMessages)
            return;

        await using var db = new PirateJimDbContext();
        
        var attachmentChannel = db.GuildAttachmentChannels
            .FirstOrDefault(x => x.ChannelId == message.Channel.Id);

        // Channel is not a attachment channel, no need to continue.
        if (attachmentChannel == null)
            return;

        if (message.Attachments.Count is >= 1 and <= 4)
        {
            foreach (var attachment in message.Attachments)
            {
                var fileName = attachment.Filename.ToLowerInvariant();

                if (!_allowedExtensions.Any(t => fileName.EndsWith(t)))
                    await message.DeleteAsync();
            }
        }
        else
        {
            await message.DeleteAsync();
        }

        await Task.CompletedTask;
    }
}