using System;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using Newtonsoft.Json;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class TestService
{
    private const string Message = "This is a test of PirateREST";
    
    [ResourceMethod(RequestMethod.GET)]
    public string GetTest(IRequest request)
    {
        var content = JsonConvert.SerializeObject(new Response(200, DateTime.UtcNow, Message), Formatting.Indented);

        return content;
    }
}