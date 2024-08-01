using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.Services;

/// <summary>
/// Represents a Service that can Initialize after the Bot has Loaded
/// </summary>
public interface IInitializableService
{
    /// <summary>
    /// Executes after the Bot has Loaded
    /// </summary>
    /// <returns></returns>
    Task InitializeAsync();
}
