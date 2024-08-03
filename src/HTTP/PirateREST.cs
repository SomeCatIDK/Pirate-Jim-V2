using GenHTTP.Engine;
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
            .Console()
            .Defaults()
            .Port(8080)
            .Run();
    }
}