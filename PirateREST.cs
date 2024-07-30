using GenHTTP.Engine;
using GenHTTP.Modules.Authentication.ApiKey;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
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
            .Port(5009)
            .Run();
    }
}


