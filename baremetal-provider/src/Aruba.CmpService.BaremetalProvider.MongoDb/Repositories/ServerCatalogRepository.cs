using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;

public class ServerCatalogRepository : IServerCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public ServerCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<InternalServerCatalog?> GetServerCatalogAsync(string? model)
    {
        var filter = Builders<ServerCatalogEntity>.Filter.Where(l => l.Model == model);
        var serverCatalogEntity = await dbContext.ServerCatalog.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);

        if (serverCatalogEntity is null)
            return null;

        return Map(serverCatalogEntity);
    }

    public async Task<IEnumerable<InternalServerCatalog>> GetAllAsync()
    {
        var filter = Builders<ServerCatalogEntity>.Filter.Empty;
        var serverCatalogEntityList = await dbContext.ServerCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        if (serverCatalogEntityList is null || serverCatalogEntityList.Count() == 0)
        {
            return new List<InternalServerCatalog>();
        }

        return serverCatalogEntityList.Select(s => Map(s)).ToList();
    }

    /// <summary>
    /// Map entity to model
    /// </summary>
    private static InternalServerCatalog Map(ServerCatalogEntity entity)
    => new InternalServerCatalog
    {
        Model = entity.Model!,
        ServerName = entity.ServerName!,
        Location = entity.Location,
        ProductCode = entity.ProductCode,
        Data = entity.Data.Select(d => new InternalServerCatalogData()
        {
            Language = d.Language,
            Connectivity = d.Connectivity,
            Cpu = d.Cpu,
            Gpu = d.Gpu,
            Hdd = d.Hdd,
            Ram = d.Ram,
        }).ToList()
    };
}
