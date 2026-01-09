
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
public interface IMCICatalogRepository
{
    /// <summary>
    /// Get mci catalog by model
    /// </summary>
    public Task<InternalMCICatalog?> GetMCICatalogAsync(string? model);

    /// <summary>
    /// Get all mci catalog
    /// </summary>
    public Task<IEnumerable<InternalMCICatalog>> GetAllAsync();
}
