using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.MCIs;
public class MCIsUseCaseTests : TestBase
{
    public MCIsUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var MCIsService = Substitute.For<IMCIsService>();
        services.AddSingleton(MCIsService);
        services.AddSingleton<MCIRenameUseCaseWrapper>();
        services.AddSingleton<MCISetAutomaticRenewUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task MCIRename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IMCIsService>();
        service.Rename(It.IsAny<RenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<MCIRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new MCIRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }
    
    [Fact]
    [Unit]
    public async Task SetAutomaticRenew_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var service = provider.GetRequiredService<IMCIsService>();
        service.SetAutomaticRenew(It.IsAny<MCISetAutomaticRenewUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<MCISetAutomaticRenewUseCaseWrapper>();
        var ret = await useCase.Execute(new MCISetAutomaticRenewUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }
}
