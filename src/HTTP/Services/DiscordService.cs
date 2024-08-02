using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json.Linq;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class DiscordService
{
    private readonly ulong _clientId;
    private readonly string _clientSecret;

    public DiscordService()
    {
        // Retrieve Discord's assigned ids to our bot.
        var clientIdString = Environment.GetEnvironmentVariable("PJ_CLIENTID");
        var clientSecret = Environment.GetEnvironmentVariable("PJ_CLIENTSECRET");

        // Enforces correct variables.
        if (clientIdString == null || clientSecret == null)
        {
            Console.WriteLine("PJ_CLIENTID and PJ_CLIENTSECRET must be set.");
            Environment.Exit(-1);
        }

        _clientSecret = clientSecret;

        if (!ulong.TryParse(clientIdString, out var clientId))
        {
            Console.WriteLine("PJ_CLIENTID must be set to a valid integer.");
            Environment.Exit(-1);
        }

        _clientId = clientId;
    }
    
    [ResourceMethod]
    public async ValueTask<IResponse?> GetDiscord(IRequest request)
    {
        // Request body requires '?code=' at the end as this is how Discord passes the codes.
        if (!request.Query.TryGetValue("code", out var code))
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("User token code is invalid."));

        // This HttpClient is used to interact with the token itself.
        using var botAuthenticatedClient = new HttpClient();
        var base64Auth = Convert.ToBase64String(new UTF8Encoding().GetBytes($"{_clientId}:{_clientSecret}"));
        botAuthenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
        
        // Exchanges code for a user token.
        var token = await ExchangeUserToken(botAuthenticatedClient, code);
        
        // Create an HttpClient authenticated with the user token.
        using var userAuthenticatedClient = new HttpClient();
        userAuthenticatedClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Fetch the user's Discord ID using their token.
        var userId = await GetUserId(userAuthenticatedClient);

        // TODO: Put the rest of the code here

        // Revokes the token so that it may not be used again. (security feature)
        await RevokeUserToken(botAuthenticatedClient, token);
        
        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, new MessageRecord(userId.ToString()));
    }

    private async ValueTask<string> ExchangeUserToken(HttpClient client, string code)
    {
        // This is the body of the POST request.
        KeyValuePair<string, string>[] tokenBodyPairs = [
            new KeyValuePair<string, string>("code", code), // This is the code returned by Discord during the first OAuth2 step.
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", "http://graybad1.net:8080/oauth2"),// FOR MY LOCAL TESTING
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

        // Check status code
        if (!result.IsSuccessStatusCode)
        {
            Console.WriteLine(await result.Content.ReadAsStringAsync());
            throw new Exception($"Discord's API sent status code {result.StatusCode.ToString()}");
        }

        // Read response body
        var resultContent = await result.Content.ReadAsStringAsync();

        // Parse the JSON and return the 'access_token' top-level field
        return JObject.Parse(resultContent)["access_token"]!.ToString();
    }

    private async Task RevokeUserToken(HttpClient client, string token)
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

        // Check status code
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync());
            throw new Exception($"Discord's API sent status code {response.StatusCode.ToString()}");
        }

        await Task.CompletedTask;
    }
    
    private async ValueTask<ulong> GetUserId(HttpClient authenticatedClient)
    {
        var userRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get, // Any Discord API endpoints we use will use GET
            RequestUri = new Uri("https://discord.com/api/v10/users/@me")
        };

        // Send request
        var userResult = await authenticatedClient.SendAsync(userRequest);

        // Check status code
        if (!userResult.IsSuccessStatusCode)
            throw new Exception($"Discord API returned status code: {userResult.StatusCode}");

        // Read JObject from the 'id' field
        var userIdObject = JObject.Parse(await userResult.Content.ReadAsStringAsync())["id"];
        
        // Ensures 'id' exists
        if (userIdObject == null)
            throw new Exception("Discord's API returned invalid data.");
        
        var userIdString = userIdObject.ToString();

        // Ensures 'id' is a number
        if (!ulong.TryParse(userIdString, out var userId))
            throw new Exception("Discord's API returned invalid data.");
        
        return userId;
    }
}