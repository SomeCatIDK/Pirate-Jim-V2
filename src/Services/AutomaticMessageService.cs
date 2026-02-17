using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

// ReSharper disable MemberCanBePrivate.Global

public class AutomaticMessageService : IService
{
    public readonly string AdvertisingMessage = $"Welcome to <#{UOChannels.Advertising}>.\n\nPlease note that you may only post advertisements **every 23 hours**. Advertisements should not promote servers that:\n- Are not relevant to the game *Unturned*.\n- Promote the usage of game cheats.\n- Actively go against [Server Hosting Rules](<https://docs.smartlydressedgames.com/en/stable/servers/server-hosting-rules.html>).\n- Use workarounds to avoid copyright claims, host bans, or other consequences regarding illegal or disallowed practices (i.e. having players manually install workshop mods).";

    public readonly string TradingMessage = $"Welcome to <#{UOChannels.Trading}>.\n\n**__Warning!__**\n You should **ONLY** trade through Steam. **DO NOT** use PayPal, Venmo, or any other service to conduct a trade. **DO NOT** trade for real money.\n\nDo not advertise:\n- Real-money trades (RMT)\n- Scams, cheats, or automation tools\n- Accounts\n\n__We cannot help you if you've lost your items, and didn't receive what you were supposed to!__\nPlease read this [Steam page](<https://help.steampowered.com/en/faqs/view/18A5-167F-C27B-64A0>).";
#if RELEASE
    public readonly string LookingForGroupMessage = $" Welcome to <#{UOChannels.LookingForGroup}>.\n\nYou may not:\n- Advertise clans or other discord servers.\n - Promote the usage of game cheats.\n - Share servers in which you are directly affiliated with the staff or owner.";
#endif
    public AutomaticMessageService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;
        bot.DiscordClient.MessageUpdated += OnMessageUpdate;
    }

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private async Task OnMessageUpdate(Cacheable<IMessage, ulong> cacheable, SocketMessage message, ISocketMessageChannel channel)
    {
        // Message was not sent by a user.
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser author)
            return;

        switch (message.Channel.Id)
        {
            case UOChannels.Modding:
                // This is more of a joke thing. We normally start a chain of messages in #modding that is just the word 'modding'.
                // Underestimated their ability to misuse this feature.
                if (message.Content.ToLowerInvariant().Contains("modding") && message.Content.Length < 12)
                    await message.AddReactionAsync(new Emoji("♥"));
                else
                    await message.RemoveAllReactionsForEmoteAsync(new Emoji("♥"));
                
                break;
        }
    }

    private async Task OnMessage(SocketMessage message)
    {
        // Message was not sent by a user.
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser author)
            return;

        switch (message.Channel.Id)
        {
            case UOChannels.Advertising:
                var advertisingMessage = await message.Channel.SendMessageAsync(AdvertisingMessage);

                ulong? oldAdvertisingMessage = await GetAndUpdateOldLastMessage(message.Channel.Id, advertisingMessage.Id);

                if (oldAdvertisingMessage != null)
                    await message.Channel.DeleteMessageAsync(oldAdvertisingMessage.Value);

                break;
            case UOChannels.Trading:
                var tradingMessage = await message.Channel.SendMessageAsync(TradingMessage);

                ulong? oldTradingMessage = await GetAndUpdateOldLastMessage(message.Channel.Id, tradingMessage.Id);

                if (oldTradingMessage != null)
                    await message.Channel.DeleteMessageAsync(oldTradingMessage.Value);

                break;
#if RELEASE
            case UOChannels.LookingForGroup:
                var lookingForGroupMessage = await message.Channel.SendMessageAsync(LookingForGroupMessage);

                ulong? oldLookingForGroupMessage = await GetAndUpdateOldLastMessage(message.Channel.Id, lookingForGroupMessage.Id);

                if (oldLookingForGroupMessage != null)
                    await message.Channel.DeleteMessageAsync(oldLookingForGroupMessage.Value);

                break;
#endif
            case UOChannels.Modding:
                // This is more of a joke thing. We normally start a chain of messages in #modding that is just the word 'modding'.
                // Underestimated their ability to misuse this feature.
                if (message.Content.ToLowerInvariant().Contains("modding") && message.Content.Length < 12)
                    await message.AddReactionAsync(new Emoji("♥"));
                
                break;
        }

        await Task.CompletedTask;
    }

    private async Task<ulong?> GetAndUpdateOldLastMessage(ulong channelId, ulong newLastMessage)
    {
        await using var db = new PirateJimDbContext();

        var lastMessageChannel = db.LastMessageChannels
            .FirstOrDefault(c => c.ChannelId == channelId);

        ulong? oldLastMessage = lastMessageChannel?.MessageId;

        if (lastMessageChannel == null)
        {
            lastMessageChannel = new LastMessageChannel
            {
                ChannelId = channelId,
                MessageId = newLastMessage
            };

            await db.LastMessageChannels.AddAsync(lastMessageChannel);
        }
        else
        {
            lastMessageChannel.MessageId = newLastMessage;

            db.LastMessageChannels.Update(lastMessageChannel);
        }

        await db.SaveChangesAsync();

        return oldLastMessage;
    }
}

