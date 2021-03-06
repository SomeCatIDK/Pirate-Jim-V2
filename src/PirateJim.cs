using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Services;

namespace SomeCatIDK.PirateJim;

public sealed class PirateJim
{
    public DiscordSocketClient DiscordClient { get; private set; } = null!;

    // These are unused at the moment, will have uses later.
    private readonly List<IService> _services = new();
    public IEnumerable<IService> Services => _services.AsReadOnly();

    public async Task Initialize()
    {
        // GuildBans is currently unused, but I don't want to forget about it later.
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers
        };

        DiscordClient = new DiscordSocketClient(discordConfig);

        DiscordClient.Log += OnLog;
        
        // Initialize the services used by the bot.
        // TODO: Make it so a guild can disable/enable these as it needs.
        _services.Add(new CommandInteractionService(this));
        _services.Add(new UserTimeoutService(this));
        
#if DEBUG
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PirateJimDebugToken"));
#else
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PirateJimToken"));
#endif
        
        await DiscordClient.StartAsync();
        
        // Keep current Task alive to prevent program from closing.
        await Task.Delay(-1);
    }
    
    // TODO: Expand into proper console/file logging.
    private static async Task OnLog(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        await Task.CompletedTask;
    }
}