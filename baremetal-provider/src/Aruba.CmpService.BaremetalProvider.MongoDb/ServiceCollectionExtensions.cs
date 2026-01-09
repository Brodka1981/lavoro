using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.MongoDb.Configuration;
using Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
using Aruba.MongoDb.Bson.Serializers;
using Aruba.MongoDb.Driver;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Aruba.CmpService.BaremetalProvider.MongoDb;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var dbSettings = configuration.GetSection("MongoDB").Get<DbSettings>(x => x.ErrorOnUnknownConfiguration = true)!;
        services.AddScoped<ITokenRepository, TokenRepository>()
            .AddScoped<ILocationMapRepository, LocationMapRepository>()
            .AddScoped<IServerCatalogRepository, ServerCatalogRepository>()
            .AddScoped<IFirewallCatalogRepository, FirewallCatalogRepository>()
            .AddScoped<ISwitchCatalogRepository, SwitchCatalogRepository>()
            .AddScoped<ISmartStorageCatalogRepository, SmartStorageCatalogRepository>()
            .AddScoped<IMCICatalogRepository, MCICatalogRepository>()
            .AddScoped<IHPCCatalogRepository, HPCCatalogRepository>()
            .AddScoped<ISwaasCatalogRepository, SwaasCatalogRepository>()
            .AddSingleton<IXmlRepository, XmlRepository>();
        services.AddMongoDbContext<BaremetalProviderDbContext>()
            .AddMongoDb(o => { o.Connect(dbSettings.ConnectionString!); o.UseDatabase(dbSettings.NameDb!); })
            .AddSingleton<Func<BaremetalProviderDbContext>>(sp => () => sp.GetService<BaremetalProviderDbContext>());

        services.ConfigureMongoDbDriver(driver
         => driver.ConfigureSerializers(serializers =>
         {
             serializers.Remove<DateTimeOffsetDecimalSerializer>();
             serializers.Add(new DateTimeOffsetSerializer(BsonType.DateTime));
             serializers.Add(new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) ||
                type.FullName!.StartsWith("Aruba.CmpService.BaremetalProvider.MongoDb.Db.Entities", StringComparison.OrdinalIgnoreCase) ||
                type.FullName!.StartsWith("Aruba.CmpService.BaremetalProvider.Abstractions.Models", StringComparison.OrdinalIgnoreCase)));
         }));

        return services;
    }
}
