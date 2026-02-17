using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Modules;

[Group("rating", "Enables and disables whether to add rating reactions to each message.")]
public class RatingChannelCommandModule : InteractionModuleBase
{
    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("enable", "Enables a rating channel.")]
    public async Task EnableAttachmentChannel([ChannelTypes(ChannelType.Text)] IChannel channel)
    {
        await using var db = new PirateJimDbContext();
        
        var ratingChannel = db.GuildRatingChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (ratingChannel == null)
        {
            db.GuildRatingChannels.Add(new GuildRatingChannel { ChannelId = channel.Id });
            await RespondAsync($"This channel is now a rating channel.");
        }
        else
            await RespondAsync($"This channel is already a rating channel.");

        await db.SaveChangesAsync();
    }
    
    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("disable", "Disables a rating channel.")]
    public async Task DisableAttachmentChannel([ChannelTypes(ChannelType.Text)] IChannel channel)
    {
        await using var db = new PirateJimDbContext();
        
        var ratingChannel = db.GuildRatingChannels
            .FirstOrDefault(x => x.ChannelId == channel.Id);

        if (ratingChannel != null)
        {
            db.GuildRatingChannels.Remove(ratingChannel);
            await RespondAsync($"This channel is no longer a rating channel.");
        }
        else
            await RespondAsync($"This channel is not a rating channel.");

        await db.SaveChangesAsync();
    }
}