using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Modules;

[Group("attachment", "Enables and disables whether to block plaintext messages and only allow embeddable attachments.")]
public class AttachmentChannelCommandModule : InteractionModuleBase
{
    [RequireOwner(Group = "ManageChannels")]
    [RequireUserPermission(ChannelPermission.ManageChannels, Group = "ManageChannels")]
    [SlashCommand("enable", "Enables an attachment channel.")]
    public async Task EnableAttachmentChannel([ChannelTypes(ChannelType.Text)] IChannel channel)
    {
        await using var db = new PirateJimDbContext();
        
        var attachmentChannel = db.GuildAttachmentChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (attachmentChannel == null)
        {
            db.GuildAttachmentChannels.Add(new GuildAttachmentChannel { ChannelId = channel.Id });
            await RespondAsync($"This channel is now an attachment channel.");
        }
        else
        {
            await RespondAsync($"This channel is already an attachment channel.");
        }

        await db.SaveChangesAsync();
    }
    
    [RequireOwner(Group = "ManageChannels")]
    [RequireUserPermission(ChannelPermission.ManageChannels, Group = "ManageChannels")]
    [SlashCommand("disable", "Disables an attachment channel.")]
    public async Task DisableAttachmentChannel([ChannelTypes(ChannelType.Text)] IChannel channel)
    {
        await using var db = new PirateJimDbContext();
        
        var attachmentChannel = db.GuildAttachmentChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (attachmentChannel != null)
        {
            db.GuildAttachmentChannels.Remove(attachmentChannel);
            await RespondAsync($"This channel is no longer an attachment channel.");
        }
        else
        {
            await RespondAsync($"This channel is not an attachment channel.");
        }

        await db.SaveChangesAsync();
    }
}