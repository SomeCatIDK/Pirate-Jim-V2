using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Services;

namespace SomeCatIDK.PirateJim;

public sealed class PirateJim
{
    public DiscordSocketClient DiscordClient { get; private set; } = null!;

    private readonly List<IService> _services = new();
    public IEnumerable<IService> Services => _services.AsReadOnly();

    public async Task Initialize()
    {
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
        };

        DiscordClient = new DiscordSocketClient(discordConfig);

        DiscordClient.Log += OnLog;
        
        _services.Add(new CommandInteractionService(this));
        _services.Add(new UserTimeoutService(this));
        
#if DEBUG
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PirateJimDebugToken"));
#else
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PirateJimToken"));
#endif
        
        await DiscordClient.StartAsync();
        await Task.Delay(-1);
    }
    
    private static async Task OnLog(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        await Task.CompletedTask;
    }
}