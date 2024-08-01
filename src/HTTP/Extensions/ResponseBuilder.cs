using System;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Extensions;

public static class ResponseBuilder
{
    public static ValueTask<IResponse?> BuildJsonResponse(this IResponseBuilder builder, ResponseStatus status, object content)
    {
        var response = new ResponseRecord((int)status, DateTime.UtcNow, builder.Request.Host + builder.Request.Target.Path, content);
        
        return new ValueTask<IResponse?>(builder
            .Status(status)
            .Content(new JsonContent(response, new JsonSerializerOptions{ WriteIndented = true }))
            .Type(FlexibleContentType.Get(ContentType.ApplicationJson))
            .Build());
    }
}