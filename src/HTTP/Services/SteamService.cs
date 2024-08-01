using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class SteamService
{
    [ResourceMethod]
    public async ValueTask<IResponse?> GetSteamItems(IRequest request)
    {
        // TODO: This will be done with a Discord OAuth2 token to retrieve the SteamID instead of using user-submitted data
        // TODO: DO NOT SHIP
        if (!request.Query.TryGetValue("SteamID", out var steamIdString))
            return await BuildInvalidSteamIdResponse(request);

        if (!ulong.TryParse(steamIdString, null, out var steamId))
            return await BuildInvalidSteamIdResponse(request);

        var client = new HttpClient()
        {
            BaseAddress = new Uri("https://steamcommunity.com")
        };
        
        var steamRequest = $"inventory/{steamId}/304930/2?l=english&count=5000";
        
        using var response = await client.GetAsync(steamRequest);
        
        if (response.StatusCode == HttpStatusCode.NotFound)
            return await BuildInvalidSteamIdResponse(request);

        if ((int) response.StatusCode != 200)
            throw new Exception($"Steam's servers returned \'{response.StatusCode}\'!");
        
        var steamResponse = await response.Content.ReadAsStringAsync();

        var gold = steamResponse.Contains(((ulong) SteamItems.GoldBowtie).ToString());
        var earlyAccess = steamResponse.Contains(((ulong) SteamItems.EarlyAccessBeret).ToString());
        
        // TODO: Bot adds roles here:
        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, new SteamItemsRecord(steamId, gold, earlyAccess));
    }

    private static async ValueTask<IResponse?> BuildInvalidSteamIdResponse(IRequest request)
    {
        return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("The query parameter \'SteamID\' must be set to a valid SteamID."));
    }
}