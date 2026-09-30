using System.Net;
using System.Security.Cryptography.X509Certificates;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Engine.Internal;
using GenHTTP.Modules.ErrorHandling;
using GenHTTP.Modules.Layouting;
using GenHTTP.Modules.Practices;
using GenHTTP.Modules.Security;
using GenHTTP.Modules.Webservices;
using SomeCatIDK.PirateJim.HTTP.Services;

namespace SomeCatIDK.PirateJim.HTTP;

public static class PirateREST
{
    public static PirateJim DiscordApp { get; private set; }
    
    public static void Initialize(PirateJim discordApp)
    {
        DiscordApp = discordApp;
        
#if RELEASE
        //TODO: This should not be hardcoded, but it will remain this way during testing.
        var certificate = X509CertificateLoader.LoadCertificateFromFile("/etc/letsencrypt/live/discordbot.smartlydressedgames.com/certificate.pfx");
#endif
        
        var testService = Layout.Create()
#if DEBUG
            .AddService<TestService>("test")
            .AddService<ErrorService>("error")
#endif
            .AddService<DiscordService>("oauth2")
            .Add(ErrorHandler.From(new JsonErrorMapper()))
            .Add(CorsPolicy.Permissive());

        Host.Create()
#if DEBUG
            .Development()
#endif
            .Handler(testService)
            .Defaults()
#if DEBUG
            .Port(8080)
#elif RELEASE
            .Bind(IPAddress.Any, 80)
            .Bind(IPAddress.Any, 443, CertificateProvider.From(certificate))
#endif
            .RunAsync().AsTask().GetAwaiter().GetResult();
    }
}