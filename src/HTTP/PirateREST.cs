using GenHTTP.Engine;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using SomeCatIDK.PirateJim.HTTP.Services;

namespace SomeCatIDK.PirateJim.HTTP;

public class PirateREST
{
    public static void Initialize()
    {
        var testService = Layout.Create()
            .AddService<TestService>("test")
            .AddService<ErrorService>("error")
            .Add(ErrorHandler.From(new JsonErrorMapper()))
            .Add(CorsPolicy.Permissive());

        Host.Create()
            #if DEBUG
            .Development()
            #endif
            .Handler(testService)
            .Console()
            .Defaults()
            .Port(8080)
            .Run();
    }
}