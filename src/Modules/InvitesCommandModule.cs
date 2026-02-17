using System.Threading.Tasks;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Modules
{
    [Group("invites", "Commands that manage invites.")]
    public class InvitesCommandModule : InteractionModuleBase
    {
        [RequireRole(UORoles.ModerationTeam)]
        [SlashCommand("clearnonrelevant", "Clears all non-relevant invites.")]
        public async Task ClearNonRelevant(int inviteCount, int belowUses)
        {
            var invites = await Context.Guild.GetInvitesAsync();

            var num = 0;

            foreach (var invite in invites)
            {
                if (invite.Inviter.Id == 192338846971723776 || invite.Inviter.Id == 178374897897177088)
                    continue;

                if (invite.Uses >= belowUses)
                    continue;

                if (num >= inviteCount)
                    break;

                await invite.DeleteAsync();
                num++;
            }

            await RespondAsync($"Deleted {num} invites.");
        }
    }
}
