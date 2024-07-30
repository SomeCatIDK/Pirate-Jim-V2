using GenHTTP.Engine;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using System.Threading.Tasks;
using GenHTTP.Modules.Webservices;
using GenHTTP.Modules.Practices;
using SomeCatIDK.PirateJim.src.Services;

namespace SomeCatIDK.PirateJim;

public class PirateREST
{
    public PirateREST Client { get; private set; }

    public PirateREST()
    {
        Client = this;
    }

    public async Task Initialize()
    {

        var testService = Layout.Create()
                .AddService<TestService>("test")
                .Add(CorsPolicy.Permissive());

        Host.Create()
            .Handler(testService)
            .Defaults()
            .Port(8080)
            .Run();
    }
}


