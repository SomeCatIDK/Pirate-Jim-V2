using System.Reflection;
using Discord.Interactions;

namespace SomeCatIDK.PirateJim.Services;

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
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), null);
        
#if DEBUG
        await _interactionService.RegisterCommandsToGuildAsync(ulong.Parse(Environment.GetEnvironmentVariable("PirateJimDebugGuild")!));
#else
        await interactionService.RegisterCommandsGloballyAsync();
#endif

        _bot.DiscordClient.InteractionCreated += async interaction =>
        {
            var context = new SocketInteractionContext(_bot.DiscordClient, interaction);
            await _interactionService.ExecuteCommandAsync(context, null);
        };
        
        await Task.CompletedTask;
    }
}