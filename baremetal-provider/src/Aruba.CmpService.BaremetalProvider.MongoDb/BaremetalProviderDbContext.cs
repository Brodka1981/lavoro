using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using Aruba.MongoDb.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb;

public class BaremetalProviderDbContext : MongoDbContext, IMongoDbContextConfigurator
{
    public static void Configure(IMongoDbContextBuilder builder, object[] configurationParameters)
    {
        if (builder is null)
            throw new ArgumentNullException(nameof(builder));

        var mongoConventions = new ConventionPack
        {
            new IgnoreExtraElementsConvention(true)
        };
        ConventionRegistry.Register("IgnoreExtraElements", mongoConventions, t => true);

        builder.AddCollection<TokenEntity>("tokens", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<DataProtectionKeyEntity>("dataprotectionkeys", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<LocationMapEntity>("locationMaps", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<ServerCatalogEntity>("serverCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<FirewallCatalogEntity>("firewallCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<SwitchCatalogEntity>("switchCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<SmartStorageCatalogEntity>("smartStorageCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<SwaasCatalogEntity>("swaasCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        builder.AddCollection<MCICatalogEntity>("mciCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });

        //TODO: check if correct
        builder.AddCollection<HPCCatalogEntity>("hpcCatalog", c =>
        {
            c.AutoMapClrType()
             .AddUniqueIndex(ib => ib.Define(idx => idx.Ascending(e => e.Id)));
        });
    }

    public IMongoCollection<TokenEntity> Tokens { get; set; } = default!;
    public IMongoCollection<DataProtectionKeyEntity> DataProtectionKeys { get; set; } = default!;
    public IMongoCollection<LocationMapEntity> LocationMaps { get; set; } = default!;
    public IMongoCollection<ServerCatalogEntity> ServerCatalog { get; set; } = default!;
    public IMongoCollection<FirewallCatalogEntity> FirewallCatalog { get; set; } = default!;
    public IMongoCollection<SwitchCatalogEntity> SwitchCatalog { get; set; } = default!;
    public IMongoCollection<SmartStorageCatalogEntity> SmartStorageCatalog { get; set; } = default!;
    public IMongoCollection<SwaasCatalogEntity> SwaasCatalog { get; set; } = default!;
    public IMongoCollection<MCICatalogEntity> MCICatalog { get; set; } = default!;
    public IMongoCollection<HPCCatalogEntity> HPCCatalog { get; set; } = default!;
}
