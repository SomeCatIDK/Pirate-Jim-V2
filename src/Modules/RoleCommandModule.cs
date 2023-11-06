using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;
using SomeCatIDK.PirateJim.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.src.Modules
{
    [Group("role", "Grants or revokes the specified role from the user.")]
    public class RoleCommandModule : InteractionModuleBase
    {
        [RequireRole(UORoles.ModerationTeam)]
        [SlashCommand("grant", "Grants the specified role to the user.")]
        public async Task GrantRole(IGuildUser user, IRole role)
        {
            if (!UORoles.GrantableRoles.Contains(role.Id)){
                await RespondAsync("The specified role cannot be granted.");
                return;
            }
            await user.AddRoleAsync(role);
            await RespondAsync($"Role granted. ✅");
        }

        [RequireRole(UORoles.ModerationTeam)]
        [SlashCommand("revoke", "Revokes the specified role from the user.")]
        public async Task RevokeRole(IGuildUser user, IRole role)
        {
            if (!UORoles.GrantableRoles.Contains(role.Id))
            {
                await RespondAsync("The specified role cannot be revoked.");
                return;
            }
            await user.RemoveRoleAsync(role);
            await RespondAsync($"Role revoked. ✅");
        }
    }
}
