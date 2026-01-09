using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Responses;
using Aruba.CmpService.BaremetalProvider.Tests.Extensions;
using Aruba.MessageBus.Models;
using Aruba.MessageBus.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases;
public class SendMessageUseCasetests : TestBase
{
    public SendMessageUseCasetests(ITestOutputHelper output) : base(output) { }
    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddSingleton<SendMessageUseCaseWrapper>();
        services.AddSingleton(Substitute.For<ILogger<SendMessageUseCase>>());
    }

    [Fact]
    [Unit]
    public async Task Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var useCase = provider.GetRequiredService<SendMessageUseCaseWrapper>();
        var request = new SendMessageUseCaseRequest()
        {
            EnvelopeToSend = new EnvelopeBuilder().Build(new())
        };
        var ret = await useCase.Execute(request).ConfigureAwait(false);
        var outbox = ret.GetOutboxMessageType<object, SendMessageUseCaseResponse>();
        ret.Should().NotBeNull();
        outbox.Should().NotBeNull();
    }
}


public class SendMessageUseCaseWrapper : SendMessageUseCase
{
    public SendMessageUseCaseWrapper(ILogger<SendMessageUseCase> logger) :
        base(logger)
    {

    }

    public async Task<HandlerResult<SendMessageUseCaseResponse>> Execute(SendMessageUseCaseRequest request)
    {
        return await base.HandleAsync(request, CancellationToken.None);
    }
}
