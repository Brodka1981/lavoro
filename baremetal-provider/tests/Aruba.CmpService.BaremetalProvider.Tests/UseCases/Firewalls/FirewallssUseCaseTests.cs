using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Firewalls.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Firewalls;
public class FirewallsUseCaseTests : TestBase
{
    public FirewallsUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var FirewallsService = Substitute.For<IFirewallsService>();
        services.AddSingleton(FirewallsService);
        services.AddSingleton<FirewallRenameUseCaseWrapper>();
        services.AddSingleton<FirewallUpdateIpUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task FirewallUpdateIp_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IFirewallsService>();
        service.UpdateIpAddress(It.IsAny<FirewallUpdateIpUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<FirewallUpdateIpUseCaseWrapper>();
        var ret = await useCase.Execute(new FirewallUpdateIpUseCaseRequest()).ConfigureAwait(false);
        ret.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task FirewallRename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IFirewallsService>();
        service.Rename(It.IsAny<RenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<FirewallRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new FirewallRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);

    }
}
