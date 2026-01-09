using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class SmartStorageCatalogRepository : ISmartStorageCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public SmartStorageCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalSmartStorageCatalog>> GetAllAsync()
    {
        var filter = Builders<SmartStorageCatalogEntity>.Filter.Empty;
        var smartStorageCatalogEntityList = await dbContext.SmartStorageCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        if (smartStorageCatalogEntityList is null || smartStorageCatalogEntityList.Count == 0)
        {
            return new List<InternalSmartStorageCatalog>();
        }

        return smartStorageCatalogEntityList.Select(s => Map(s)).ToList();
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private static InternalSmartStorageCatalog Map(SmartStorageCatalogEntity entity)
        => new InternalSmartStorageCatalog
        {
            Code = entity.Code,
            Snapshot = entity.Snapshot,
            Size = entity.Size,
            Replica = entity.Replica,
        };
}
