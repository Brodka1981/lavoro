
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
public class MCICatalogRepository : IMCICatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public MCICatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalMCICatalog>> GetAllAsync()
    {
        var filter = Builders<MCICatalogEntity>.Filter.Empty;
        var serverCatalogEntityList = await dbContext.MCICatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        var catalog = MapCatalog(serverCatalogEntityList);

        return await Task.FromResult<IEnumerable<InternalMCICatalog>>(catalog).ConfigureAwait(false);
    }

    public async Task<InternalMCICatalog?> GetMCICatalogAsync(string? model)
    {
        var catalog = await GetAllAsync().ConfigureAwait(false);

        if (catalog == null) return null;

        var filteredCatalog = catalog.FirstOrDefault(c => c.Model!.Equals(model, StringComparison.OrdinalIgnoreCase));

        if (filteredCatalog == null) return null;

        return await Task.FromResult<InternalMCICatalog>(filteredCatalog).ConfigureAwait(false);
    }

    private static List<InternalMCICatalog> MapCatalog(List<MCICatalogEntity> entities)
    {
        List<InternalMCICatalog> catalog = new List<InternalMCICatalog>();

        foreach (MCICatalogEntity entity in entities)
        {
            var option = new InternalMCICatalog()
            {
                BundleConfigurationCode = entity.BundleConfigurationCode,
                Model = entity.ServerCode,
                ServerName = entity.ServerName,
                Category = entity.Category,
                Data = MapServerData(entity.ServerData.ToList()),
                Firewall = new InternalMCICatalogFirewall() 
                        { 
                            Model = entity.FirewallCode,
                            Data = MapFirewallData(entity.FirewallData.ToList()),
                        },
            };

            catalog.Add(option);
        }

        return catalog;
    }

    private static List<InternalMCIFirewallData> MapFirewallData(List<MCICatalogFirewallDataEntity> firewallDataEntity)
    {
        List<InternalMCIFirewallData> firewallData = new List<InternalMCIFirewallData>();

        foreach (MCICatalogFirewallDataEntity data in firewallDataEntity)
        {
            var item = new InternalMCIFirewallData()
            {
                Language = data.Language,
                FirewallName = data.FirewallName,
            };

            firewallData.Add(item);
        }

        return firewallData;
    }

    private static List<InternalMCICatalogData> MapServerData(List<MCICatalogServerDataEntity> serverDataEntity)
    {
        List<InternalMCICatalogData> serverData = new List<InternalMCICatalogData>();

        foreach(MCICatalogServerDataEntity data in serverDataEntity)
        {
            var item = new InternalMCICatalogData()
            {
                Language = data.Language,
                Cpu = data.Cpu,
                HardwareName = data.HardwareName,
                Hdd = data.Hdd,
                NodeNumber = data.NodeNumber,
                Ram = data.Ram,
            };

            serverData.Add(item);
        }

        return serverData;
    }
 }
