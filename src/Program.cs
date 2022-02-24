namespace SomeCatIDK.PirateJim;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var bot = new PirateJim();
        await bot.Initialize();
    }
}