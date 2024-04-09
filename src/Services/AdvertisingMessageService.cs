
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Services;
using SomeCatIDK.PirateJim.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.src.Services;

public class AdvertisingMessageService : IService
{
    public const string Message = "Welcome to #advertising.\n\nPlease note that you may only post advertisements **every 23 hours**. Advertisements should not promote servers that:\n- Are not relevant to the game *Unturned*.\n- Promote the usage of game cheats.\n- Actively go against [Server Hosting Rules](<https://docs.smartlydressedgames.com/en/stable/servers/server-hosting-rules.html>).\n- Use workarounds to avoid copyright claims, host bans, or other consequences regarding illegal or disallowed practices (i.e. having players manually install workshop mods).";

    public ulong? LastMessage;

    public AdvertisingMessageService(PirateJim bot)
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

        if (message.Channel.Id != UOChannels.Advertising)
            return;

        var msg = await message.Channel.SendMessageAsync(Message);

        if (LastMessage != null)
            await message.Channel.DeleteMessageAsync(LastMessage.Value);

        LastMessage = msg.Id;

        await Task.CompletedTask;
        //idk
    }
}

