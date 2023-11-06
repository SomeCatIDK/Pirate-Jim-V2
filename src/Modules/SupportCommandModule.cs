using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

namespace SomeCatIDK.PirateJim.Modules
{
    public class SupportCommandModule : InteractionModuleBase
    {
        [SlashCommand("article", "Finds a support article relevant to the search query.")]
        public async Task GetZendeskArticle(string search)
        {
            try
            {
                var request = WebRequest.CreateHttp($"https://support.smartlydressedgames.com/api/v2/help_center/articles/search?query={Uri.EscapeDataString(search)}");
                
                request.Method = "GET";
                request.ContentType = "application/json";

                var res = (HttpWebResponse)(await request.GetResponseAsync());

                using var readStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8);
                
                var zendeskRes = JObject.Parse(await readStream.ReadToEndAsync());
                var articles = (JArray)zendeskRes["results"]!;
                    
                if (articles.Count == 0)
                {
                    await RespondAsync("Could not find the proper article.");
                    return;
                }
                    
                var embed = new EmbedBuilder
                {
                    Title = "Found SDG Article",
                    Description = $"[{articles[0]["name"]!.ToObject<string>()}]({articles[0]["html_url"]!.ToObject<string>()})",
                    Color = Color.DarkGrey
                };
                    
                await RespondAsync(embed: embed.Build());
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); 
                Console.WriteLine(e.StackTrace);
                
                await RespondAsync("Failed to get an article from Zendesk. (internal error)");
            }
        }
    }
}
