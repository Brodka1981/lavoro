using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class FirewallCatalogRepository : IFirewallCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public FirewallCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalFirewallCatalog>> GetAllAsync()
    {
        var filter = Builders<FirewallCatalogEntity>.Filter.Empty;
        var firewallCatalogEntityList = await dbContext.FirewallCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        if (firewallCatalogEntityList is null || firewallCatalogEntityList.Count == 0)
        {
            return new List<InternalFirewallCatalog>();
        }

        return firewallCatalogEntityList.Select(s => Map(s)).ToList();
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private static InternalFirewallCatalog Map(FirewallCatalogEntity entity)
        => new InternalFirewallCatalog
        {
            Code = entity.Code,
            Mode = entity.Mode,
        };
}
