using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Helpers;

public static class SteamHelper
{
    public static async ValueTask<SteamItemsRecord> GetSteamItems(ulong steamId, bool verified)
    {
        var client = new HttpClient();

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"https://steamcommunity.com/inventory/{steamId}/304930/2?l=english&count=5000")
        };

        var response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.Forbidden)
            return new SteamItemsRecord(steamId, true, verified, []);
        
        response.EnsureSuccessStatusCode();

        if (!verified)
            return new SteamItemsRecord(steamId, false, verified, []);
        
        var data = await response.Content.ReadAsStringAsync();

        var items = new List<ESteamItem>();
        
        if (data.Contains("Gold Bowtie"))
            items.Add(ESteamItem.GoldBowtie);
        
        if (data.Contains("Early Access Beret"))
            items.Add(ESteamItem.EarlyAccessBeret);
        
        if (data.Contains("Debugger's Beret"))
            items.Add(ESteamItem.DebuggersBeret);
        
        if (data.Contains("Experienced Beret"))
            items.Add(ESteamItem.ExperiencedBeret);
        
        if (data.Contains("White Hat"))
            items.Add(ESteamItem.WhiteHat);
        
        if (data.Contains("Crimson Beret"))
            items.Add(ESteamItem.CrimsonBeret);

        return new SteamItemsRecord(steamId, false, verified, items.ToArray());
    }
}