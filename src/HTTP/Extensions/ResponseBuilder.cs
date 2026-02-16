using System;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Serializers.Json;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Extensions;

public static class ResponseBuilder
{
    public static ValueTask<IResponse?> BuildJsonResponse(this IRequest request, ResponseStatus status, object content)
    {
        // Creates a C# object representing the content of the message.
        var response = new ResponseRecord($"{(int)status} - {status.ToString()}", DateTime.UtcNow, request.Host + request.Target.Path, content);
        
        // Build a pretty-printed JSON response with application/json header
        return new ValueTask<IResponse?>(request
            .Respond()
            .Status(status)
            .Content(new JsonContent(response, new JsonSerializerOptions { WriteIndented = true }))
            .Type(FlexibleContentType.Get(ContentType.ApplicationJson))
            .Build());
    }
}