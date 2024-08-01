using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;
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
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildBans | GatewayIntents.GuildMessages | GatewayIntents.GuildMembers | GatewayIntents.MessageContent
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

        DiscordClient.MessageReceived += InviteChecker;
        
        _services.Add(new RemoveInvalidGuideTagService(this));
        
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PJ_TOKEN"));
        
        await DiscordClient.StartAsync();

        await DiscordClient.SetGameAsync("V2 time!");

        foreach (var service in _services)
        {
            if (service is IInitializableService initializableService)
                await initializableService.InitializeAsync();
        }

        // Keep current Task alive to prevent program from closing.
        await Task.Delay(-1);
    }

    // TODO: move this somewhere else and refine it.
    private static async Task InviteChecker(SocketMessage message)
    {
        if (message.Author is not SocketGuildUser author)
            return;

        if (message.Channel.Id == UOChannels.Advertising)
            return;
        
        var roles = new[]
        {
            UORoles.SDG,
            UORoles.ModerationTeam,
            UORoles.Supporter
        };

        // Checks to see if the user has any roles defined the in `roles` local variable.
        // This syntax is hellish, but it's only one line.
        if (author.Roles.Select(x => x.Id).Any(roles.Contains))
            return;

        var content = message.Content.ToLowerInvariant();

        if (content.Contains("discord.gg/") || content.Contains("discord.com/invite/"))
            return;
        
        await message.DeleteAsync();
    }
    
    
    // TODO: Expand into proper console/file logging.
    private static async Task OnLog(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        await Task.CompletedTask;
    }
}