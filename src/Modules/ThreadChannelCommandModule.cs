using Discord;
using Discord.Interactions;
using SomeCatIDK.PirateJim.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.src.Modules;

[Group("thread", "Commands that manage threads.")]
public class ThreadChannelCommandModule : InteractionModuleBase
{
    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("close", "Closes the thread.")]
    public async Task CloseThread(string reason)
    {
        if (Context.Channel is not IThreadChannel channel)
        {
            await RespondAsync("The channel in which this command was executed is not a thread.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("Thread Closed")
            .WithDescription($"This thread has been closed by a moderator.\n\n**Reason:** {reason}")
            .WithColor(Color.DarkGrey)
            .Build(), ephemeral: false);

        await channel.ModifyAsync((ThreadChannelProperties properties) =>
        {
            properties.Locked = true;
        });
    }

    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("slowmode", "Applies a slowmode to the thread.")]
    public async Task SetThreadSlowmode(int seconds)
    {
        if (Context.Channel is not IThreadChannel channel)
        {
            await RespondAsync("The channel in which this command was executed is not a thread.", ephemeral: true);
            return;
        }

        await RespondAsync($"Slowmode set to {seconds} seconds.", ephemeral: true);

        await channel.ModifyAsync((ThreadChannelProperties properties) =>
        {
            properties.SlowModeInterval = seconds;

        });
    }

    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("threadban", "Gives a user the threads banned role.")]
    public async Task ThreadBanUser(IGuildUser user, string reason)
    {
        if (user.RoleIds.Contains(UORoles.ModerationTeam))
            await RespondAsync($"The specified user is a moderator.", ephemeral: true);

        await user.AddRoleAsync(UORoles.ThreadsBanned);

        await RespondAsync($"Threads banned **{user.GlobalName}** (`{user.Id}`).", ephemeral: false);


        var modlogChannel = await Context.Guild.GetChannelAsync(379004358806863883UL) as ITextChannel;

        await modlogChannel!.SendMessageAsync(embed: new EmbedBuilder()
            .WithTitle("threadsban")
            .WithDescription($"**Offender:** {user.GlobalName} (`{user.Id}`).\n**Reason:** {reason}\n**Responsible moderator:** {Context.User.GlobalName} (`{Context.User.Id}`)")
            .WithCurrentTimestamp()
            .WithColor(Color.DarkGrey)
            .Build());

        try
        {
            var dm = await user.CreateDMChannelAsync();
            await dm.SendMessageAsync($"You were threads banned.\n\n**Reason:** {reason}\n**Responsible moderator:** {Context.User.GlobalName} (`{Context.User.Id}`)");
        }
        catch
        {
            await RespondAsync($"An error occurred when attempting to DM the user.", ephemeral: true);
        }
    }

    [RequireRole(UORoles.ModerationTeam)]
    [SlashCommand("threadunban", "Revokes the threads banned role from the specified user.")]
    public async Task ThreadUnbanUser(IGuildUser user, string? reason)
    {
        await user.RemoveRoleAsync(UORoles.ThreadsBanned);

        var modlogChannel = await Context.Guild.GetChannelAsync(379004358806863883UL) as ITextChannel;

        await modlogChannel!.SendMessageAsync(embed: new EmbedBuilder()
           .WithTitle("threadsunban")
           .WithDescription($"**Offender:** {user.GlobalName} (`{user.Id}`)." + reason != null ? $"\n**Reason:** {reason}" : "\n**Reason:** *No specified reason.*" + $"\n**Responsible moderator:** {Context.User.GlobalName} (`{Context.User.Id}`)")
           .WithCurrentTimestamp()
           .WithColor(Color.Green)
           .Build());

        await RespondAsync($"Threads unbanned **{user.GlobalName}** (`{user.Id}`).", ephemeral: false);
    }
}

