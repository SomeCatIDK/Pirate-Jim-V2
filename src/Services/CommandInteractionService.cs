using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SomeCatIDK.PirateJim.Services;

// This service registers all commands with Discord's Interaction feature-set.
public class CommandInteractionService : IService
{ 
    private readonly PirateJim _bot;
    private readonly InteractionService _interactionService;
    
    public CommandInteractionService(PirateJim bot)
    {
        _bot = bot;
        _interactionService = new InteractionService(_bot.DiscordClient.Rest);
        
        _bot.DiscordClient.Ready += OnReady;
    }
    
    private async Task OnReady()
    {
        // Initialize all "modules" in the entry assembly.
        // TODO: GetEntryAssembly() will return the incorrect assembly if the bot is used as a library instead of an executable.
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        
        // TODO: Use global initializer instead.
        
//#if DEBUG
        // In a test environment, register commands to test guild.
        // Used instead of global call because the global call can take up to an hour to fully propagate to all guilds.
        await _interactionService.RegisterCommandsToGuildAsync(ulong.Parse(Environment.GetEnvironmentVariable("PJ_GUILD")!));
//#else
        // In a production environment, register commands to all guilds.
        //await _interactionService.RegisterCommandsGloballyAsync();
//#endif

        // According to Discord.NET, this should not be required, but it doesn't work without it.
        // Thanks StackOverflow for the snippet <3
        _bot.DiscordClient.InteractionCreated += async interaction =>
        {
            var context = new SocketInteractionContext(_bot.DiscordClient, interaction);
            await _interactionService.ExecuteCommandAsync(context, null);
        };
        
        await Task.CompletedTask;
    }
}