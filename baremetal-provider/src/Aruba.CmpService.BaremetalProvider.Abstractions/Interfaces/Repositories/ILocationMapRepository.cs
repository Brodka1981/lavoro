using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface ILocationMapRepository
{
    /// <summary>
    /// Get location map by legacy value
    /// </summary>
    public Task<LocationMap?> GetLocationMapAsync(string? legacyValue);

}
