using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Jobs;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Jobs;
using Aruba.CmpService.BaremetalProvider.MongoDb.Configuration;
using Aruba.Hangfire;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Dependencies;

[ExcludeFromCodeCoverage(Justification = "It's a class for dependency injection with facade methods")]

public static class SchedulerCollectionExtensions
{
    public static IServiceCollection AddHangFire(this IServiceCollection services, IConfiguration configuration)
    {
        configuration.ThrowIfNull();
        var mongoOptions = configuration.GetSection("MongoDB").Get<DbSettings>(x => x.ErrorOnUnknownConfiguration = true)!;

        // Add Hangfire service
        services.AddHangfireService(config =>
                config.UseMongoDBStorage(mongoOptions.ConnectionString!, mongoOptions.NameDb!),
            workerCount: 5);
        //add local server
        services.AddHangfireServer();
        _ = services.AddJobs();
        return services;
    }

    private static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddScoped<IDeleteTokensJob, DeleteTokensJob>();
        return services;
    }
}
