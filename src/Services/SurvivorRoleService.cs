using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Model;

namespace SomeCatIDK.PirateJim.Services;

[Service]
public class SurvivorRoleService : IInitializableService
{
    public SurvivorRoleService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnMessage;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    // ReSharper disable once MemberCanBeMadeStatic.Local
    private async Task OnMessage(SocketMessage message)
    {
        if (message.Source != MessageSource.User)
            return;

        // Not sure if a cast check is required, but may as well.
        if (message.Author is not SocketGuildUser author)
            return;

        var role = author.Roles.FirstOrDefault(x => x.Id == UORoles.Survivor);

        if (role == null)
            await author.AddRoleAsync(UORoles.Survivor);
        
        await Task.CompletedTask;
    }
}