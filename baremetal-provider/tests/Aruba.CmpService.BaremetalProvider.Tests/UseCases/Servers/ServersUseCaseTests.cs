using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Servers;
public class ServersUseCaseTests : TestBase
{
    public ServersUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var serversService = Substitute.For<IServersService>();
        services.AddSingleton(serversService);
        services.AddSingleton<DeletePleskLicenseUseCaseWrapper>();
        services.AddSingleton<ServerRenameUseCaseWrapper>();
        services.AddSingleton<ServerRestartUseCaseWrapper>();
        services.AddSingleton<ServerUpdateIpUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task DeletePleskLicense_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IServersService>();
        service.DeletePleskLicense(It.IsAny<DeletePleskLicenseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<DeletePleskLicenseUseCaseWrapper>();
        var ret = await useCase.Execute(new DeletePleskLicenseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);

    }

    [Fact]
    [Unit]
    public async Task ServerRestart_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IServersService>();
        service.Restart(It.IsAny<ServerRestartUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<ServerRestartUseCaseWrapper>();
        var ret = await useCase.Execute(new ServerRestartUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task ServerUpdateIp_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IServersService>();
        service.UpdateIpAddress(It.IsAny<ServerUpdateIpUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<ServerUpdateIpUseCaseWrapper>();
        var ret = await useCase.Execute(new ServerUpdateIpUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task ServerRename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IServersService>();
        service.Rename(It.IsAny<RenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<ServerRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new ServerRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);

    }
}
