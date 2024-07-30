using System.Threading;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim;

public static class Program
{
    private static async Task Main(string[] args)
    {
        ThreadPool.QueueUserWorkItem(async (o) =>
        {
            var botClient = new PirateJim();
            await botClient.Initialize();
        });

        var RESTClient = new PirateREST();
        await RESTClient.Initialize();
    }
}