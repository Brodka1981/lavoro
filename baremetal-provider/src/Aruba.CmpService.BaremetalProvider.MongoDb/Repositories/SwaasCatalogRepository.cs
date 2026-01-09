using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class SwaasCatalogRepository : ISwaasCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public SwaasCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalSwaasCatalog>> GetAllAsync()
    {
        var filter = Builders<SwaasCatalogEntity>.Filter.Empty;
        var swaasCatalogEntityList = await dbContext.SwaasCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        if (swaasCatalogEntityList is null || swaasCatalogEntityList.Count == 0)
        {
            return new List<InternalSwaasCatalog>();
        }

        return swaasCatalogEntityList.Select(s => Map(s)).ToList();
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private static InternalSwaasCatalog Map(SwaasCatalogEntity entity)
        => new InternalSwaasCatalog
        {
            Code = entity.Code,
            ItemId = entity.ItemId,
            LinkedDevicesCount = entity.LinkedDevicesCount,
            Data = entity.Data.Select(d => new InternalSwaasCatalogData()
            {
                Language = d.Language,
                Model = d.Model,
            }).ToList()
        };
}
