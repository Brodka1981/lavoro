using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Switches;
public class SwitchesUseCaseTests : TestBase
{
    public SwitchesUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var SwitchesService = Substitute.For<ISwitchesService>();
        services.AddSingleton(SwitchesService);
        services.AddSingleton<SwitchRenameUseCaseWrapper>();
        services.AddSingleton<SwitchSetAutomaticRenewUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task SwitchRename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwitchesService>();
        service.Rename(It.IsAny<RenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SwitchRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new SwitchRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);

    }

    [Fact]
    [Unit]
    public async Task SwitchSetAutomaticRenewUseCase_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<ISwitchesService>();
        service.SetAutomaticRenew(It.IsAny<SetAutomaticRenewUseCaseRequest>(), It.IsAny<CancellationToken>())
            .ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SwitchSetAutomaticRenewUseCaseWrapper>();
        var ret = await useCase.Execute(new SwitchSetAutomaticRenewUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);

    }
}
