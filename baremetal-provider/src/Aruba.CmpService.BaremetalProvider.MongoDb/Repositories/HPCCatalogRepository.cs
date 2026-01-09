
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
public class HPCCatalogRepository : IHPCCatalogRepository
{
    private readonly BaremetalProviderDbContext dbContext;

    public HPCCatalogRepository(BaremetalProviderDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IEnumerable<InternalHPCCatalog>> GetAllAsync()
    {
        var filter = Builders<HPCCatalogEntity>.Filter.Empty;
        var serverCatalogEntityList = await dbContext.HPCCatalog.Find(filter).ToListAsync().ConfigureAwait(false);

        var catalog = MapCatalog(serverCatalogEntityList);

        return await Task.FromResult<IEnumerable<InternalHPCCatalog>>(catalog).ConfigureAwait(false);
    }

    public async Task<InternalHPCCatalog?> GetHPCCatalogAsync(string? model)
    {
        var catalog = await GetAllAsync().ConfigureAwait(false);

        if (catalog == null) return null;

        var filteredCatalog = catalog.FirstOrDefault(c => c.Model!.Equals(model, StringComparison.OrdinalIgnoreCase));

        if (filteredCatalog == null) return null;

        return await Task.FromResult<InternalHPCCatalog>(filteredCatalog).ConfigureAwait(false);
    }

    private static List<InternalHPCCatalog> MapCatalog(List<HPCCatalogEntity> entities)
    {
        List<InternalHPCCatalog> catalog = new List<InternalHPCCatalog>();

        foreach (HPCCatalogEntity entity in entities)
        {
            var option = new InternalHPCCatalog()
            {
                BundleConfigurationCode = entity.BundleConfigurationCode,
                Model = entity.ServerCode,
                ServerName = entity.ServerName,
                Category = entity.Category,
                Data = MapServerData(entity.ServerData.ToList()),
                Firewall = new InternalHPCCatalogFirewall()
                {
                    Model = entity.FirewallCode,
                    Data = MapFirewallData(entity.FirewallData.ToList()),
                },
            };

            catalog.Add(option);
        }

        return catalog;
    }

    private static List<InternalHPCFirewallData> MapFirewallData(List<HPCCatalogFirewallDataEntity> firewallDataEntity)
    {
        List<InternalHPCFirewallData> firewallData = new List<InternalHPCFirewallData>();

        foreach (HPCCatalogFirewallDataEntity data in firewallDataEntity)
        {
            var item = new InternalHPCFirewallData()
            {
                Language = data.Language,
                FirewallName = data.FirewallName,
            };

            firewallData.Add(item);
        }

        return firewallData;
    }

    private static List<InternalHPCCatalogData> MapServerData(List<HPCCatalogServerDataEntity> serverDataEntity)
    {
        List<InternalHPCCatalogData> serverData = new List<InternalHPCCatalogData>();

        foreach (HPCCatalogServerDataEntity data in serverDataEntity)
        {
            var item = new InternalHPCCatalogData()
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
