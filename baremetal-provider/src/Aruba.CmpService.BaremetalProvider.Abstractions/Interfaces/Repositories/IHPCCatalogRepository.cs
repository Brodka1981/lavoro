
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
public interface IHPCCatalogRepository
{
    /// <summary>
    /// Get mci catalog by model
    /// </summary>
    public Task<InternalHPCCatalog?> GetHPCCatalogAsync(string? model);

    /// <summary>
    /// Get all mci catalog
    /// </summary>
    public Task<IEnumerable<InternalHPCCatalog>> GetAllAsync();
}
