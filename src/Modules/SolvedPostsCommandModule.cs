using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.HTTP;
using SomeCatIDK.PirateJim.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.Modules;

public class SolvedPostsCommandModule : InteractionModuleBase
{
    [SlashCommand("solved", "Marks the current support thread as solved.", ignoreGroupNames: true)]
    public async Task Solved()
    {
        // Not sure what the best way to get the service here is
        if (PirateREST.DiscordApp.Services.FirstOrDefault(s => s is SolvedPostsService) is not SolvedPostsService service) return;

        await service.MarkAsSolvedAsync(null, Context.Channel, Context.User, response =>
        {
            if (string.IsNullOrEmpty(response)) return RespondAsync("You've marked this post as solved.", flags: MessageFlags.Ephemeral);
            return RespondAsync(response, flags: MessageFlags.Ephemeral);
        });
    }
}
