using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;
using static Aruba.CmpService.BaremetalProvider.Tests.UseCases.Internal.InternalUseCaseWrappers;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Internal;

public class InternalUseCaseTests : TestBase
{
    public InternalUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var internalService = Substitute.For<IInternalService>();
        services.AddSingleton(internalService);
        services.AddSingleton<InternalAdminGetResourcesUseCaseWrapper>();
        services.AddSingleton<InternalAutomaticRenewUseCaseWrapper>();
        services.AddSingleton<InternalAutorechargeUseCaseWrapper>();
        services.AddSingleton<InternalGetRegionsUseCaseWrapper>();
        services.AddSingleton<InternalGetResourcesUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task InternalAdminGetResources_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IInternalService>();
        service.AdminGetAllResources(It.IsAny<InternalAdminGetResourcesUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult<IEnumerable<BaseLegacyResource>>());

        var useCase = provider.GetRequiredService<InternalAdminGetResourcesUseCaseWrapper>();
        var ret = await useCase.Execute(new InternalAdminGetResourcesUseCaseRequest()).ConfigureAwait(false);
    }

    [Fact]
    [Unit]
    public async Task InternalGetResources_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IInternalService>();
        service.GetAllResources(It.IsAny<InternalGetResourcesUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult<IEnumerable<LegacyResource>>());

        var useCase = provider.GetRequiredService<InternalGetResourcesUseCaseWrapper>();
        var ret = await useCase.Execute(new InternalGetResourcesUseCaseRequest()).ConfigureAwait(false);
    }

    [Fact]
    [Unit]
    public async Task InternalAutorecharge_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IInternalService>();
        service.GetAutorecharge(It.IsAny<InternalAutorechargeUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult<AutorechargeResponse>());

        var useCase = provider.GetRequiredService<InternalAutorechargeUseCaseWrapper>();
        var ret = await useCase.Execute(new InternalAutorechargeUseCaseRequest()).ConfigureAwait(false);
    }

    [Fact]
    [Unit]
    public async Task InternalGetRegions_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IInternalService>();
        service.GetRegions(It.IsAny<InternalGetRegionsUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult<IEnumerable<Region>>());

        var useCase = provider.GetRequiredService<InternalGetRegionsUseCaseWrapper>();
        var ret = await useCase.Execute(new InternalGetRegionsUseCaseRequest()).ConfigureAwait(false);
    }

    [Fact]
    [Unit]
    public async Task InternalAutomaticRenew_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IInternalService>();
        service.UpsertAutomaticRenew(It.IsAny<InternalAutomaticRenewUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult<IEnumerable<BaseLegacyResource>>());

        var useCase = provider.GetRequiredService<InternalAutomaticRenewUseCaseWrapper>();
        var ret = await useCase.Execute(new InternalAutomaticRenewUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }
}
