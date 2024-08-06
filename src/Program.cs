using System.Threading;
using System.Threading.Tasks;
using SomeCatIDK.PirateJim.HTTP;

namespace SomeCatIDK.PirateJim;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var botClient = new PirateJim();
        
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await botClient.Initialize();
        });
        
        PirateREST.Initialize(botClient);

        await Task.CompletedTask;
    }
}