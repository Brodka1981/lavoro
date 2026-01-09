using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class SwitchCatalogRepository : ISwitchCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public SwitchCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalSwitchCatalog>> GetAllAsync()
    {
        var filter = Builders<SwitchCatalogEntity>.Filter.Empty;
        var switchCatalogEntityList = await dbContext.SwitchCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        if (switchCatalogEntityList is null || switchCatalogEntityList.Count() == 0)
        {
            return new List<InternalSwitchCatalog>();
        }

        return switchCatalogEntityList.Select(s => Map(s)).ToList();
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private static InternalSwitchCatalog Map(SwitchCatalogEntity entity)
    => new InternalSwitchCatalog
    {
        Code = entity.Code,
        Location = entity.Location,
        Ports = entity.Ports,
    };
}
