using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Tests.UseCases.Servers;
using Aruba.MessageBus.UseCases;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases;
public class BaseNoResponseTests : TestBase
{
    public BaseNoResponseTests(ITestOutputHelper output) : base(output) { }
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<TestBaseUseCaseWrapper>();
        services.AddSingleton<ServerRenameUseCaseWrapper>();
        services.AddSingleton<ServerRestartUseCaseWrapper>();
        services.AddSingleton<ServerUpdateIpUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var useCase = provider.GetRequiredService<TestBaseUseCaseWrapper>();
        var ret = await useCase.Execute(new TestBaseUseCaseRequest()).ConfigureAwait(false);
        ret.Should().NotBeNull();

    }
}


public class TestBaseUseCaseRequest : BaseUseCaseRequest
{

}
public class TestBaseUseCaseResponse : EmptyMessageBusResponse
{

}
public class TestBaseUseCaseWrapper : NoResponseBaseUseCase<TestBaseUseCaseRequest, TestBaseUseCaseResponse>
{
    protected override async Task<ServiceResult> ExecuteService(TestBaseUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new ServiceResult());
    }
    public async Task<HandlerResult<TestBaseUseCaseResponse>> Execute(TestBaseUseCaseRequest request)
    {
        return await base.HandleAsync(request, CancellationToken.None).ConfigureAwait(false);
    }

    protected override async Task AdditionalNullChecks(TestBaseUseCaseRequest request)
    {
        await base.AdditionalNullChecks(request);
    }
}
