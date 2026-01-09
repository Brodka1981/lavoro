using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.Jobs;
public class DeleteTokenJobTests : TestBase
{
    public DeleteTokenJobTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton(Substitute.For<ITokenProvider>());
        services.AddSingleton(Substitute.For<ILogger<DeleteTokensJob>>());
        services.AddSingleton(Substitute.For<IRecurringJobManager>());
        services.AddSingleton<DeleteTokensJob>();
    }

    [Fact]
    [Unit]
    public async Task DeleteTokenJob_Execute()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var job = provider.GetRequiredService<DeleteTokensJob>();

        job.Execute();
    }
}
