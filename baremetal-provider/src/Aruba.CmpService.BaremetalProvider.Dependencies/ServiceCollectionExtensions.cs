using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.HttpProviders;
using Aruba.CmpService.BaremetalProvider.Infrastructure;
using Aruba.CmpService.BaremetalProvider.MongoDb;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aruba.CmpService.BaremetalProvider.Dependencies;

[ExcludeFromCodeCoverage(Justification = "It's a class for dependency injection with facade methods")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaremetalProviderServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddAbstractions(configuration, isDevelopment);

        services.AddRepositories(configuration);

        services.AddHttpProviders(configuration);

        services.AddInfrastructureServices(configuration);

        return services;
    }
}
