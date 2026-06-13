using Autofac;
using Autofac.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SomeCatIDK.PirateJim.Extensions;
using SomeCatIDK.PirateJim.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ContainerBuilder = Autofac.ContainerBuilder;

namespace SomeCatIDK.PirateJim;

public sealed class PirateJim
{
    public DiscordSocketClient DiscordClient { get; private set; } = null!;

    public IContainer ServiceContainer { get; private set; } = null!;

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
        var containerBuilder = new ContainerBuilder();
        
        containerBuilder.Populate(new ServiceCollection());
        containerBuilder.RegisterInstance(this);

        var serviceTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute<ServiceAttribute>() != null);
        foreach (var serviceType in serviceTypes)
        {
            var serviceLifetime = serviceType.GetCustomAttribute<ServiceAttribute>()!.Lifetime;

            containerBuilder.RegisterType(serviceType)
                .AsImplementedInterfaces().AsSelf()
                .InstancePerServiceLifetime(serviceLifetime);
        }

        ServiceContainer = containerBuilder.Build();

        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PJ_TOKEN"));
        await DiscordClient.StartAsync();

        await DiscordClient.SetGameAsync("Yarrrr!");

        var tasks = ServiceContainer.Resolve<IEnumerable<IInitializableService>>().Select(s => s.InitializeAsync());
        await Task.WhenAll(tasks);

        // Keep current Task alive to prevent program from closing.
        await Task.Delay(-1);
    }
    
    private static readonly Lock LogLock = new();
    
    // TODO: Expand into proper console/file logging.
    private static async Task OnLog(LogMessage msg)
    {
        lock (LogLock)
            Console.WriteLine(msg.ToString());

        await Task.CompletedTask;
    }
}