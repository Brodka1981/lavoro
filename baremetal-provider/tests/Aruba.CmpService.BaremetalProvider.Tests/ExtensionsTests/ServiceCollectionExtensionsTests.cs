using Aruba.CmpService.BaremetalProvider.Dependencies;
using Aruba.CmpService.BaremetalProvider.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class ServiceCollectionExtensionsTests :
    TestBase
{
    public ServiceCollectionExtensionsTests(ITestOutputHelper output)
        : base(output) { }

    private IServiceProvider ConfigureServiceCollection()
    {
        var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.tests.json")
                 .AddEnvironmentVariables()
                 .Build();

        var serviceCollection = CreateServiceCollection();
        serviceCollection.AddInfrastructureServices(configuration);
        serviceCollection.AddBaremetalProviderServices(configuration, true);
        return serviceCollection.BuildServiceProvider();
    }
}
