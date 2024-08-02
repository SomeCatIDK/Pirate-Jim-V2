//using System.Threading;
using System.Threading.Tasks;
using SomeCatIDK.PirateJim.HTTP;

namespace SomeCatIDK.PirateJim;

public static class Program
{
    private static async Task Main(string[] args)
    {
        
        /*
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            var botClient = new PirateJim();
            await botClient.Initialize();
        });
        */
        
        var restClient = new PirateREST();
        PirateREST.Initialize();

        await Task.CompletedTask;
    }
}