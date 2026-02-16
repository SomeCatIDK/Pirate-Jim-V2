using System;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.ErrorHandling;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP;

public class JsonErrorMapper : IErrorMapper<Exception>
{
    // This method is called every time an exception is thrown inside a thread managed by GenHTTP.
    public async ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        Console.WriteLine(error.Message + error.StackTrace);
        
        return await request
            .BuildJsonResponse(ResponseStatus.InternalServerError, new MessageRecord("An internal error occured, please report this to an administrator: " + error.Message));
    }

    // 404
    public async ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        return await request
            .BuildJsonResponse(ResponseStatus.NotFound, new MessageRecord("The requested endpoint does not exist."));
    }
}