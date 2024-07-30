﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers.Json;
using Newtonsoft.Json;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP;

public class JsonErrorMapper : IErrorMapper<Exception>
{
    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        var response = new Response(500, DateTime.UtcNow, JsonConvert.SerializeObject(error));

        return new ValueTask<IResponse?>(request.Respond()
            .Status(ResponseStatus.InternalServerError)
            .Content(new JsonContent(response, JsonSerializerOptions.Default))
            .Type(FlexibleContentType.Get(ContentType.ApplicationJson))
            .Build());

    }

    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var response = new Response(404, DateTime.UtcNow, JsonConvert.SerializeObject("The requested endpoint does not exist."));
        
        return new ValueTask<IResponse?>(request.Respond()
            .Status(ResponseStatus.NotFound)
            .Content(new JsonContent(response, new JsonSerializerOptions()))
            .Type(FlexibleContentType.Get(ContentType.ApplicationJson))
            .Build());
    }
}