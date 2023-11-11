﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Newtonsoft.Json.Linq;

namespace SomeCatIDK.PirateJim.Modules;

[Group("support", "Provides useful commands for survivors and supporters.")]
public class SupportCommandModule : InteractionModuleBase
{
    [SlashCommand("article", "Finds a SDG support article relevant to the search query.")]
    public async Task GetZendeskArticle(string search)
    {
        try
        {
            // TODO: Replace with HttpClient
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
                Title = "Found SDG Support Article",
                Description = $"**[{articles[0]["name"]!.ToObject<string>()}]({articles[0]["html_url"]!.ToObject<string>()})**",
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

    [SlashCommand("docs", "Finds a SDG documentation page relevant to the search query.")]
    public async Task GetDocsArticle(string search)
    {
        try
        {
            // TODO: Replace with HttpClient
            var request = WebRequest.CreateHttp($"https://readthedocs.org/api/v3/search/?format=json&q=project:unturned{Uri.EscapeDataString($" {search}")}");

            request.Method = "GET";
            request.ContentType = "application/json";

            var res = (HttpWebResponse)(await request.GetResponseAsync());

            using var readStream = new StreamReader(res.GetResponseStream(), Encoding.UTF8);

            var docsRes = JObject.Parse(await readStream.ReadToEndAsync());
            var articles = (JArray)docsRes["results"]!;

            if (articles.Count == 0)
            {
                await RespondAsync("Could not find the proper documentation page.");
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = "Found SDG Documentation Article",
                Description = $"**[{articles[0]["title"]!.ToObject<string>()}](https://docs.smartlydressedgames.com{articles[0]["path"]!.ToObject<string>()})**",
                Color = Color.DarkGrey
            };

            await RespondAsync(embed: embed.Build());

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            await RespondAsync("Failed to get an article from Read the Docs. (internal error)");
        }
    }

    [SlashCommand("unity", "Replies with the latest Unity version for Unturned.")]
    public async Task GetUnityVersion()
    {
        try
        {
            using var client = new HttpClient();
            var content = (await client.GetStringAsync("https://raw.githubusercontent.com/Unturned-Datamining/Unturned-Datamining/linux-client-preview/.unityversion"));

            await RespondAsync($"The 'preview' branch of Unturned is using Unity version **{content}**.");
        }
        catch(Exception e) 
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            await RespondAsync("Failed to get Unity version from GitHub Unturned-Datamining. (internal error)");
        }
    }

    [SlashCommand("gameban", "Sends information on gamebans and how they should be appealed.")]
    public async Task SendGamebanInfo(IGuildUser? user = null)
    {
        string alert = "### Do not use this Discord for appealing gamebans. 🛑\n[Contact BattlEye](https://www.battleye.com/support/) to appeal gamebans. Further information is provided on the [SDG Support Site](https://support.smartlydressedgames.com/hc/en-us/articles/12082986788884)";

        if (user != null)
            await RespondAsync($"{user.Mention}\n{alert}");
        else
            await RespondAsync(alert);
    }
}