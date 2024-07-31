using System;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP;

public class JsonErrorMapper : IErrorMapper<Exception>
{
    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        Console.WriteLine(error.Message + error.StackTrace);
        var response = request.Respond()
            .GetJsonResponse(ResponseStatus.InternalServerError, new ErrorRecord(error.Message, error.StackTrace ?? string.Empty));

        return response;
    }

    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var response = request.Respond()
            .GetJsonResponse(ResponseStatus.NotFound, new MessageRecord("The requested endpoint does not exist."));

        return response;
    }
}