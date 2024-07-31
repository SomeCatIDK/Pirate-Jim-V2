using System;
using System.Text.Json;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using GenHTTP.Modules.Webservices;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class TestService
{
    private const string Message = "This is a test of PirateREST";
    
    [ResourceMethod]
    public IResponse GetTest(IRequest request)
    {
        var response = new ResponseRecord(200, DateTime.UtcNow, new MessageRecord(Message));
        
        return request.Respond()
            .Content(new JsonContent(response, JsonSerializerOptions.Default))
            .Type(FlexibleContentType.Get(ContentType.ApplicationJson))
            .Build();
    }
}