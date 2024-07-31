using System;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class ErrorService
{
    [ResourceMethod]
    public IResponse GetError(IRequest request)
    {
        throw new Exception("This is a test of the error handler");
    }
}