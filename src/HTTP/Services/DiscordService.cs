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
    private ulong _clientId = 0;
    private string _clientSecret = string.Empty;
    
    [ResourceMethod(RequestMethod.GET)]
    public async ValueTask<IResponse?> GetDiscord(IRequest request)
    {
        if (_clientId == 0)
            _clientId = ulong.Parse(Environment.GetEnvironmentVariable("PJ_CLIENTID")!);

        if (_clientSecret == string.Empty)
            _clientSecret = Environment.GetEnvironmentVariable("PJ_CLIENTSECRET") ?? throw new Exception("Client secret not implemented!");
        
        if (!request.Query.TryGetValue("code", out var code))
            return await request.Respond().BuildJsonResponse(ResponseStatus.BadRequest, new MessageRecord("User token code is invalid."));

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
            RequestUri = new Uri("https://discord.com/api/oauth2/token")
        };

        var result = await client.SendAsync(tokenAuthRequest);

        if (!result.IsSuccessStatusCode)
            throw new Exception($"Discord's API sent status code {result.StatusCode.ToString()}");
        
        var resultContent = await result.Content.ReadAsStringAsync();

        var token = JObject.Parse(resultContent)["access_token"]!.ToString();
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var connectionsRequest = new HttpRequestMessage()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://discord.com/api/v10/users/@me/connections")
        };

        var connectionsResponse = await client.SendAsync(connectionsRequest);

        var jsonResponse = JObject.Parse(await connectionsResponse.Content.ReadAsStringAsync());

        foreach (var obj in jsonResponse)
        {
            
        }
        
        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, await connectionsResponse.Content.ReadAsStringAsync());
    }
}