using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;

public interface IFirewallCatalogRepository
{
    /// <summary>
    /// Get all firewall catalog
    /// </summary>
    public Task<IEnumerable<InternalFirewallCatalog>> GetAllAsync();
}
