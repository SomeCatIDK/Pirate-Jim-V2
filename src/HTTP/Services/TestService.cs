#if DEBUG
using System.Threading.Tasks;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;
using SomeCatIDK.PirateJim.HTTP.Extensions;
using SomeCatIDK.PirateJim.HTTP.Model;

namespace SomeCatIDK.PirateJim.HTTP.Services;

public class TestService
{
    private const string Message = "This is a test of PirateREST";
    
    [ResourceMethod]
    public async ValueTask<IResponse?> GetTest(IRequest request)
    {
        return await request.Respond().BuildJsonResponse(ResponseStatus.OK, new MessageRecord(Message));
    }
}
#endif