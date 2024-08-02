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
        var clientIdString = Environment.GetEnvironmentVariable("PJ_CLIENTID");
        var clientSecret = Environment.GetEnvironmentVariable("PJ_CLIENTSECRET");

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
        if (!request.Query.TryGetValue("code", out var code))
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("User token code is invalid."));

        var token = await ExchangeUserToken(code);
        
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var userId = await GetUserId(client);

        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, new MessageRecord(userId.ToString()));
    }

    private async ValueTask<string> ExchangeUserToken(string code)
    {
        using var client = new HttpClient();

        var base64Auth = Convert.ToBase64String(new UTF8Encoding().GetBytes($"{_clientId}:{_clientSecret}"));
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);
        
        KeyValuePair<string, string>[] tokenBodyPairs = [
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("redirect_uri", "http://graybad1.net:8080/oauth2"),// FOR MY LOCAL TESTING
        ];

        var tokenAuthBody = new FormUrlEncodedContent(tokenBodyPairs);
        
        var tokenAuthRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Post,
            Content = tokenAuthBody,
            RequestUri = new Uri("https://discord.com/api/oauth2/token"),
        };

        var result = await client.SendAsync(tokenAuthRequest);

        if (!result.IsSuccessStatusCode)
        {
            Console.WriteLine(await result.Content.ReadAsStringAsync());
            throw new Exception($"Discord's API sent status code {result.StatusCode.ToString()}");
        }

        var resultContent = await result.Content.ReadAsStringAsync();

        return JObject.Parse(resultContent)["access_token"]!.ToString();
    }

    private async ValueTask<ulong> GetUserId(HttpClient authenticatedClient)
    {
        var userRequest = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://discord.com/api/v10/users/@me")
        };

        var userResult = await authenticatedClient.SendAsync(userRequest);

        if (!userResult.IsSuccessStatusCode)
            throw new Exception($"Discord API returned status code: {userResult.StatusCode}");

        var userIdObject = JObject.Parse(await userResult.Content.ReadAsStringAsync())["id"];
        
        if (userIdObject == null)
            throw new Exception("Discord's API returned invalid data.");
        
        var userIdString = userIdObject.ToString();

        if (!ulong.TryParse(userIdString, out var userId))
            throw new Exception("Discord's API returned invalid data.");
        
        return userId;
    }
}