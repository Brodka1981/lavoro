using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Reflection;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Configuration;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Middlewares;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders;

[ExcludeFromCodeCoverage(Justification = "It's a class for dependency injection with facade methods")]
public static class HttpProvidesServiceCollectionExtensions
{

    public static IServiceCollection AddHttpProviders(this IServiceCollection services, IConfiguration configuration)
    {
        //Http Client provider registration
        var externalEndpointsOptions = configuration.GetRequiredSection(nameof(ExternalEndpoints)).Get<ExternalEndpoints>();
        var properties = typeof(BaremetalHttpClientNames).GetProperties(BindingFlags.Public | BindingFlags.Static).Select(s => s.Name).ToArray();
        foreach (var property in properties)
        {
            services.InnerAddHttpClients(property, externalEndpointsOptions[property]);
        }

        //Middlewares Registration
        services.AddMiddlewares();

        //Providers Registration
        services.AddProviders(configuration);


        return services;
    }

    private static IServiceCollection AddProviders(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProjectProvider, ProjectProvider>();
        services.AddScoped<IProfileProvider, ProfileProvider>();
        services.AddScoped<ICatalogueProvider, CatalogueProvider>();
        services.AddScoped<IServersProvider, ServersProvider>();
        services.AddScoped<IFirewallsProvider, FirewallsProvider>();
        services.AddScoped<ISwitchesProvider, SwitchesProvider>();
        services.AddScoped<ISwaasesProvider, SwaasesProvider>();
        services.AddScoped<IPaymentsProvider, PaymentsProvider>();
        services.AddScoped<ISmartStoragesProvider, SmartStoragesProvider>();
        services.AddScoped<IMCIsProvider, MCIsProvider>();
        services.AddScoped<IHPCsProvider, HPCsProvider>();

        services.AddScoped<IInternalLegacyProvider, InternalLegacyProvider>();
        services.AddScoped<IAdminLegacyProvider, AdminLegacyProvider>();

        return services;
    }

    private static IServiceCollection AddMiddlewares(this IServiceCollection services)
    {
        services.AddTransient<LegacyHandler>();
        services.AddTransient<AdminLegacyHandler>();
        return services;
    }

    private static void InnerAddHttpClients(this IServiceCollection services, string name, string baseAddress)
    {
        services.InnerAddHttpClient($"{name}_user", baseAddress);
        services.InnerAddHttpClient($"{name}_service", baseAddress);
    }

    private static void InnerAddHttpClient(this IServiceCollection services, string name, string baseAddress)
    {
        var builder = services.AddHttpClient(name, httpClient =>
        {
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri(baseAddress);
            httpClient.Timeout = TimeSpan.FromSeconds(60);
        });

        if (name.StartsWith(BaremetalHttpClientNames.LegacyProvider.Value, StringComparison.OrdinalIgnoreCase))
        {
            builder.AddHttpMessageHandler<LegacyHandler>();
        }
        else if (name.StartsWith(BaremetalHttpClientNames.AdminLegacyProvider.Value, StringComparison.OrdinalIgnoreCase))
        {
            builder.AddHttpMessageHandler<AdminLegacyHandler>();
        }
        else if (name.EndsWith("_user"))
        {
            builder.AddHeaderPropagation(o =>
            {
                o.Headers.Add("Authorization");
            });
        }
    }
}
