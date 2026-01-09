using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure;

[ExcludeFromCodeCoverage(Justification = "It's a class for dependency injection with facade methods")]
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var assembly = typeof(BaseSearchFiltersRequest<>).Assembly;
        services.AddQueryHandlers(assembly);
        //services.AddAudit(configuration);
        services.AddServices();
        services.AddEncryption();
        return services;
    }

    //public static IServiceCollection AddAudit(this IServiceCollection services, IConfiguration configuration)
    //{
    //    var auditingOptions = configuration.GetSection(nameof(AuditingOptions)).Get<AuditingOptions>();
    //    services.AddAuditing(auditingOptions);
    //    return services;
    //}

    public static IServiceCollection AddEncryption(this IServiceCollection services)
    {
        services.AddDataProtection()
            .SetApplicationName("BaremetalProvider");
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(sp =>
        {
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = sp.GetService<IXmlRepository>();
            });
        });

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IServersService, ServersService>();
        services.AddScoped<IInternalService, InternalService>();
        services.AddScoped<ISwitchesService, SwitchesService>();
        services.AddScoped<ISwaasesService, SwaasesService>();
        services.AddScoped<IFirewallsService, FirewallsService>();
        services.AddScoped<IPaymentsService, PaymentsService>();
        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<IEncryptProvider, EncryptProvider>();
        services.AddScoped<ISmartStoragesService, SmartStoragesService>();
        services.AddScoped<IMCIsService, MCIsService>();
        services.AddScoped<IHPCsService, HPCsService>();
        return services;
    }

    public static IServiceCollection AddQueryHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.AddSingleton<IQueryService, QueryService>();

        var assemblies = new Assembly[2] { assembly, Assembly.GetExecutingAssembly() };
        var assemblyTypes = assemblies.SelectMany(sm => sm.GetTypes())
            .Where(w => !w.IsAbstract && w.BaseType != null)
            .ToArray();

        var typesToFilter = assemblyTypes.Where(w => !w.IsAbstract && w.BaseType != null).ToList();

        foreach (var typeToFilter in typesToFilter)
        {
            var implementedInterfaces = typeToFilter.GetInterfaces().Where(w => w.IsGenericType && w.GetGenericTypeDefinition() == typeof(IQueryHandler<,>)).ToList();
            if (implementedInterfaces.Any())
            {
                foreach (var implementedInterface in implementedInterfaces)
                {
                    services.AddScoped(implementedInterface, typeToFilter);
                }
            }
        }

        return services;
    }
}
