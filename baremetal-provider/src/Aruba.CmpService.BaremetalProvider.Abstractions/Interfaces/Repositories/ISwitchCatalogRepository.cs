using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface ISwitchCatalogRepository
{
    /// <summary>
    /// Get all switch catalog
    /// </summary>
    public Task<IEnumerable<InternalSwitchCatalog>> GetAllAsync();
}
