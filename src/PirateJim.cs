using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Services;

namespace SomeCatIDK.PirateJim;

public sealed class PirateJim
{
    public DiscordSocketClient DiscordClient { get; private set; } = null!;

    // These are unused at the moment, will have uses later.
    private readonly List<IService> _services = [];
    public IEnumerable<IService> Services => _services.AsReadOnly();

    public async Task Initialize()
    {
        var discordConfig = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true
        };

        DiscordClient = new DiscordSocketClient(discordConfig);

        DiscordClient.Log += OnLog;
        
        // Initialize the services used by the bot.
        // TODO: Make it so a guild can disable/enable these as it needs.
        _services.Add(new CommandInteractionService(this));
        _services.Add(new UserTimeoutService(this));
        _services.Add(new AttachmentChannelService(this));
        _services.Add(new AppealsAutoCloseService(this));
        _services.Add(new RatingChannelService(this));
        _services.Add(new SurvivorRoleService(this));
        _services.Add(new AutomaticMessageService(this));
        _services.Add(new RemoveInvalidGuideTagService(this));
        
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PJ_TOKEN"));
        
        await DiscordClient.StartAsync();

        await DiscordClient.SetGameAsync("Yarrrr!");
      
        foreach (var service in _services)
        {
            if (service is IInitializableService initializableService)
                await initializableService.InitializeAsync();
        }

        // Keep current Task alive to prevent program from closing.
        await Task.Delay(-1);
    }
    
    private static readonly object LogLock = new object();
    
    // TODO: Expand into proper console/file logging.
    private static async Task OnLog(LogMessage msg)
    {
        lock (LogLock)
            Console.WriteLine(msg.ToString());

        await Task.CompletedTask;
    }
}