using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Swaases;

public class SwaasesUseCaseTests : TestBase
{
    public SwaasesUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var SwaasesService = Substitute.For<ISwaasesService>();
        services.AddSingleton(SwaasesService);
        services.AddSingleton<SwaasRenameUseCaseWrapper>();
        services.AddSingleton<SwaasSetAutomaticRenewUseCaseWrapper>();
        services.AddSingleton<VirtualSwitchAddUseCaseWrapper>();
        services.AddSingleton<VirtualSwitchDeleteUseCaseWrapper>();
        services.AddSingleton<VirtualSwitchEditUseCaseWrapper>();
        services.AddSingleton<VirtualSwitchLinkAddUseCaseWrapper>();
        services.AddSingleton<VirtualSwitchLinkDeleteUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task SwaasRename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.Rename(It.IsAny<RenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SwaasRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new SwaasRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task SwaasSetAutomaticRenewUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.SetAutomaticRenew(It.IsAny<SetAutomaticRenewUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SwaasSetAutomaticRenewUseCaseWrapper>();
        var ret = await useCase.Execute(new SwaasSetAutomaticRenewUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task VirtualSwitchAddUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.AddVirtualSwitch(It.IsAny<VirtualSwitchAddUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<VirtualSwitch>());

        var useCase = provider.GetRequiredService<VirtualSwitchAddUseCaseWrapper>();
        var ret = await useCase.Execute(new VirtualSwitchAddUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task VirtualSwitchDeleteUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.DeleteVirtualSwitch(It.IsAny<VirtualSwitchDeleteUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<VirtualSwitch>());

        var useCase = provider.GetRequiredService<VirtualSwitchDeleteUseCaseWrapper>();
        var ret = await useCase.Execute(new VirtualSwitchDeleteUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task VirtualSwitchEditUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.EditVirtualSwitch(It.IsAny<VirtualSwitchEditUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<VirtualSwitch>());

        var useCase = provider.GetRequiredService<VirtualSwitchEditUseCaseWrapper>();
        var ret = await useCase.Execute(new VirtualSwitchEditUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task VirtualSwitchLinkAddUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.AddVirtualSwitchLink(It.IsAny<VirtualSwitchLinkAddUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<VirtualSwitchLink>());

        var useCase = provider.GetRequiredService<VirtualSwitchLinkAddUseCaseWrapper>();
        var ret = await useCase.Execute(new VirtualSwitchLinkAddUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task VirtualSwitchLinkDeleteUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwaasesService>();
        service.DeleteVirtualSwitchLink(It.IsAny<VirtualSwitchLinkDeleteUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult<VirtualSwitchLink>());

        var useCase = provider.GetRequiredService<VirtualSwitchLinkDeleteUseCaseWrapper>();
        var ret = await useCase.Execute(new VirtualSwitchLinkDeleteUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }
}
