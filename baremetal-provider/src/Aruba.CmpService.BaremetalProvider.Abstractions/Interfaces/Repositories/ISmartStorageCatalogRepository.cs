using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface ISmartStorageCatalogRepository
{
    /// <summary>
    /// Get all smart storage catalog
    /// </summary>
    public Task<IEnumerable<InternalSmartStorageCatalog>> GetAllAsync();
}
