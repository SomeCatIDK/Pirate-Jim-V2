using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json.Linq;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Helpers;
using SomeCatIDK.PirateJim.HTTP.Model;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class DiscordService
{
    private readonly ulong _clientId;
    private readonly string _clientSecret;

    public DiscordService()
    {
        // Retrieve Discord's assigned ids to our bot.
        var clientIdString = Environment.GetEnvironmentVariable("PJ_ID");
        var clientSecret = Environment.GetEnvironmentVariable("PJ_SECRET");

        // Enforces correct variables.
        if (clientIdString == null || clientSecret == null)
        {
            Console.WriteLine("PJ_ID and PJ_SECRET must be set.");
            Environment.Exit(-1);
        }

        _clientSecret = clientSecret;

        if (!ulong.TryParse(clientIdString, out var clientId))
        {
            Console.WriteLine("PJ_ID must be set to a valid integer.");
            Environment.Exit(-1);
        }

        _clientId = clientId;
    }
    
    [ResourceMethod]
    public async ValueTask<IResponse?> GetDiscord(IRequest request)
    {
        // Request body requires '?code=' at the end as this is how Discord passes the codes.
        // 'code' is used by OAuth2 to fetch a user token.
        if (!request.Query.TryGetValue("code", out var code))
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("Code was not found in query, please use the link found on Discord to try again."));

        // This HttpClient is used to interact with the token itself.
        // This client is authenticated using the bot's information.
        using var botAuthenticatedClient = new HttpClient();
        var base64Auth = Convert.ToBase64String(new UTF8Encoding().GetBytes($"{_clientId}:{_clientSecret}"));
        botAuthenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
        
        // Exchanges code for a user token.
        var token = await ExchangeUserToken(botAuthenticatedClient, code);
        
        if (token == null)
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("Authentication failed, the data submitted to Discord's server is invalid. This will happen if you refresh the page. Please use the link found on Discord to try again."));
        
        // Create an HttpClient authenticated with the user token.
        using var userAuthenticatedClient = new HttpClient();
        userAuthenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Fetch the user's Discord ID using their token.
        var userId = await GetUserId(userAuthenticatedClient);

        var connections = await GetUserConnections(userAuthenticatedClient);

        if (connections == null)
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("Authentication failed, Discord's servers may be down. Please report this to an administrator."));
        
        var records = new List<SteamItemsRecord>();
        
        foreach (var steamConnection in connections.Accounts.Where(x => x.EAccountType == EAccountType.Steam))
        {
            var items = await SteamHelper.GetSteamItems(ulong.Parse(steamConnection.Id), steamConnection.Verified);
            
            if (items == null)
                return await request.Respond().BuildJsonResponse(ResponseStatus.InternalServerError, new MessageRecord("Steam's servers encountered an internal error. Steam may be down for maintenance, please try again later."));
            
            records.Add(items);
        }
        
        // TODO: Put the rest of the code here

        // Revokes the token so that it may not be used again. (security feature)
        // If the user uses this API again, the user must reauthorize through OAuth2 to issue a new token.
        await RevokeUserToken(botAuthenticatedClient, token);

        var guild = PirateREST.DiscordApp.DiscordClient.Guilds.First();
        
        var user = guild.GetUser(userId);

        var steamAccountsPretty = new List<FinalRolesRecord>();
        
        foreach (var steamAccount in records)
        {
            var roleNames = new List<string>();

            if (steamAccount.Items.Contains(ESteamItem.CrimsonBeret))
            {
                roleNames.Add("Crimson Beret");
                await user.AddRoleAsync(UORoles.CrimsonBeret);
            }

            if (steamAccount.Items.Contains(ESteamItem.DebuggersBeret))
            {
                roleNames.Add("Debugger");
                await user.AddRoleAsync(UORoles.Debugger);
            }

            if (steamAccount.Items.Contains(ESteamItem.ExperiencedBeret))
            {
                roleNames.Add("Experienced");
                await user.AddRoleAsync(UORoles.Experienced);
            }

            if (steamAccount.Items.Contains(ESteamItem.GoldBowtie))
            {
                roleNames.Add("Gold");
                await user.AddRoleAsync(UORoles.Gold);
            }

            if (steamAccount.Items.Contains(ESteamItem.WhiteHat)) 
            {
                roleNames.Add("White Hatter");
                await user.AddRoleAsync(UORoles.WhiteHatter);
            }

            if (steamAccount.Items.Contains(ESteamItem.EarlyAccessBeret)) 
            { 
                roleNames.Add("Early Access"); 
                await user.AddRoleAsync(UORoles.EarlyAccess);
            }

            var message = string.Empty;
            
            if (steamAccount.InventoryPrivate)
                message = "Your Steam inventory is set to private or friends-only. Please change your inventory privacy setting to public. ";

            if (!steamAccount.Verified)
                message += "The link between your Steam and Discord account is not verified. Please unlink and relink your accounts through Discord's settings. ";

            if (!steamAccount.InventoryPrivate && steamAccount.Verified && steamAccount.Items.Length == 0)
                message = "No eligible roles were found. Please review the roles on our Discord server. If you believe this is an error, please contact an administrator.";
            
            message = message != string.Empty ? message.Remove(message.Length - 1, 1) : "No errors encountered";
            
            steamAccountsPretty.Add(new FinalRolesRecord(steamAccount.SteamId, message, roleNames.ToArray()));
        }

        if (steamAccountsPretty.Count == 0)
        {
            return await request.Respond().BuildJsonResponse(ResponseStatus.OK, new MessageRecord("No connected accounts were found. Please link your accounts for which you wish to receive roles."));
        }
        
        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, steamAccountsPretty);
    }

    private static async ValueTask<string?> ExchangeUserToken(HttpClient client, string code)
    {
        // This is the body of the POST request.
        KeyValuePair<string, string>[] tokenBodyPairs = [
            new ("code", code), // This is the code returned by Discord during the first OAuth2 step.
            new ("grant_type", "authorization_code"),
            new ("redirect_uri", "https://discordbot.smartlydressedgames.com/oauth2")
        ];

        // Formats the body with 'x-www-form-urlencoded'
        var tokenAuthBody = new FormUrlEncodedContent(tokenBodyPairs);
        
        var tokenAuthRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, // All token interactions use POST
            Content = tokenAuthBody,
            RequestUri = new Uri("https://discord.com/api/oauth2/token"),
        };

        var result = await client.SendAsync(tokenAuthRequest);

        if (!result.IsSuccessStatusCode)
            return null;
        
        // Read response body
        var resultContent = await result.Content.ReadAsStringAsync();

        // Parse the JSON and return the 'access_token' top-level field
        return JObject.Parse(resultContent)["access_token"]!.ToString();
    }

    private static async Task RevokeUserToken(HttpClient client, string token)
    {
        // This is the body of the POST request.
        KeyValuePair<string, string>[] tokenBodyPairs =
        [
            new KeyValuePair<string, string>("token", token), // The token to be revoked
            new KeyValuePair<string, string>("token_type_hint", "access_token")
        ];
        
        // Formats the body with 'x-www-form-urlencoded'
        var tokenAuthBody = new FormUrlEncodedContent(tokenBodyPairs);
        
        var revokeRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post, // All token interactions use POST
            Content = tokenAuthBody,
            RequestUri = new Uri("https://discord.com/api/oauth2/token/revoke")
        };

        var response = await client.SendAsync(revokeRequest);
        
        response.EnsureSuccessStatusCode();
    }

    private static async ValueTask<ulong> GetUserId(HttpClient client)
    {
        var userRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get, // Any non-OAuth2 Discord API endpoints we use will use GET
            RequestUri = new Uri("https://discord.com/api/v10/users/@me") // '@me' will work as it returns the user identified by the submitted token. 
        };

        // Send request
        var userResult = await client.SendAsync(userRequest);

        // Check status code
        if (!userResult.IsSuccessStatusCode)
            return 0;

        // Read JObject from the 'id' field
        var userIdObject = JObject.Parse(await userResult.Content.ReadAsStringAsync())["id"];
        
        var userIdString = userIdObject!.ToString();
        
        return ulong.Parse(userIdString);
    }

    private static async ValueTask<DiscordUserConnections?> GetUserConnections(HttpClient client)
    {
        var connectionsRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get, // Any non-OAuth2 Discord API endpoints we use will use GET
            RequestUri = new Uri("https://discord.com/api/v10/users/@me/connections") // '@me' will work as it returns the user identified by the submitted token. 
        };

        var result = await client.SendAsync(connectionsRequest);

        if (!result.IsSuccessStatusCode)
            return null;

        var jsonArray = JArray.Parse(await result.Content.ReadAsStringAsync());

        var connections = new List<ConnectedAccount>();
        
        foreach (var item in jsonArray)
        {
            var type = item["type"]!.Value<string>();
            var id = item["id"]!.Value<string>();
            var verified = item["verified"]!.Value<bool>();
            
            switch (type)
            {
                case "steam":
                    connections.Add(new ConnectedAccount(EAccountType.Steam, id!, verified));
                    break;
                case "youtube":
                    connections.Add(new ConnectedAccount(EAccountType.YouTube, id!, verified));
                    break;
                case "twitch":
                    connections.Add(new ConnectedAccount(EAccountType.Twitch, id!, verified));
                    break;
                default:
                    continue;
            }
        }
        
        return new DiscordUserConnections(connections.ToArray());
    }
}