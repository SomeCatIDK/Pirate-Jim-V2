using System;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP;

public class JsonErrorMapper : IErrorMapper<Exception>
{
    // This method is called every time an exception is thrown inside a thread managed by GenHTTP.
    public async ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        Console.WriteLine(error.Message + error.StackTrace);
        
        return await request.Respond()
            .BuildJsonResponse(ResponseStatus.InternalServerError, new ErrorRecord(error.Message, error.StackTrace ?? string.Empty));
    }

    // 404
    public async ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        return await request.Respond()
            .BuildJsonResponse(ResponseStatus.NotFound, new MessageRecord("The requested endpoint does not exist."));
    }
}