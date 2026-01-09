using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class LocationMapRepository : ILocationMapRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public LocationMapRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<LocationMap?> GetLocationMapAsync(string? legacyValue)
    {
        var filter = Builders<LocationMapEntity>.Filter.Where(l => l.LegacyValue == legacyValue);
        var locationMapEntity = await dbContext.LocationMaps.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

        return Map(locationMapEntity);
    }

    /// <summary>
    /// Map entity to model
    private static LocationMap? Map(LocationMapEntity? entity)
    {
        if (entity != null)
        {
            return new LocationMap()
            {
                Id = entity.Id!,
                Value = entity.Value!,
                LegacyValue = entity.LegacyValue!,
            };
        }
        return null;
    }
}
