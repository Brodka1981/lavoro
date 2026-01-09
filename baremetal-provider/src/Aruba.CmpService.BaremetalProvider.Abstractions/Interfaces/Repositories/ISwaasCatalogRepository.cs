using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface ISwaasCatalogRepository
{
    /// <summary>
    /// Get all swaas catalog
    /// </summary>
    public Task<IEnumerable<InternalSwaasCatalog>> GetAllAsync();
}
