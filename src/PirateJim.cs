﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Services;
using SomeCatIDK.PirateJim.src.Services;

namespace SomeCatIDK.PirateJim;

public sealed class PirateJim
{
    public const ulong BotId = 553384844919439360;
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
        _services.Add(new RatingChannelService(this));
        _services.Add(new SurvivorRoleService(this));
        _services.Add(new ChannelAlertService(this));
        
        await DiscordClient.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("PJ_TOKEN"));
        
        await DiscordClient.StartAsync();

        await DiscordClient.SetGameAsync("V2 time!");
        
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