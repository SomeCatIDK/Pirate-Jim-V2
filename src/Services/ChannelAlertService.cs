using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;
using SomeCatIDK.PirateJim.Services;
using SomeCatIDK.PirateJim.src.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.src.Services
{
    //Service to keep an alert in #trading and #advertising up.
    public class ChannelAlertService : IService
    {
        public const string TradingAlertMessage = "**Warning!** __You should **ONLY** trade through Steam.__ Do not use PayPal, Venmo, or any other service to conduct a trade. Do not trade for real money. If you are scammed and lose your items or money, we cannot help you.\nAll trades should be conducted through Steam's trading window, and all Steam Community Market listings should be handled through the Steam Community market. Do not use multiple services together, and do not use any service other than those available through Steam.\nNo begging for free items. Begging for items may result in you being warned or banned.\n**__Contact Steam__ if you've lost your items, and didn't receive what you were supposed to!** Steam can only help you if you only used Steam's services. We cannot personally help you if you've been scammed. [Link to FAQ from Steam](https://help.steampowered.com/en/faqs/view/18A5-167F-C27B-64A0)";
        public const string AdvertingAlertMessage = "Your advertisement can **ONLY** be posted every **23 hours**! If the same advertisement is posted multiple times within that period, you may risk getting muted.";

        public ChannelAlertService(PirateJim bot)
        {
            bot.DiscordClient.MessageReceived += OnMessage;
        }

        private async Task OnMessage(SocketMessage message)
        {
            await using var db = new PirateJimDbContext();

            if (message.Channel.Id == UOChannels.Trading && !message.Author.IsBot)
            {
                await message.Channel.SendMessageAsync(TradingAlertMessage);
                CachedMessage? cachedMessage = db.CachedMessages.FirstOrDefault(x => x.UserId == PirateJim.BotId && x.ChannelId == UOChannels.Trading);
                if (cachedMessage != null)
                {
                    await message.Channel.DeleteMessageAsync(cachedMessage.MessageId);
                    db.CachedMessages.Remove(cachedMessage);
                }

            }



            await Task.CompletedTask;
        }
    }
}
