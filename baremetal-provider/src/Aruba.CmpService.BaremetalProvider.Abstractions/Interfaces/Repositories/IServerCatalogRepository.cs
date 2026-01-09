using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface IServerCatalogRepository
{
    /// <summary>
    /// Get server catalog by model
    /// </summary>
    public Task<InternalServerCatalog?> GetServerCatalogAsync(string? model);

    /// <summary>
    /// Get all server catalog
    /// </summary>
    public Task<IEnumerable<InternalServerCatalog>> GetAllAsync();
}
