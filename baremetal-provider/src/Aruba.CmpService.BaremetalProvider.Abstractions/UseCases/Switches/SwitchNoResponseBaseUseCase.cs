using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.MessageBus.UseCases;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches;
public abstract class SwitchNoResponseBaseUseCase<TRequest, TResponse> : NoResponseBaseUseCase<TRequest, TResponse>
    where TRequest : BaseUseCaseRequest
    where TResponse : EmptyMessageBusResponse
{
    protected ISwitchesService SwitchesService { get; }

    protected SwitchNoResponseBaseUseCase(ISwitchesService switchesService)
    {
        SwitchesService = switchesService;
    }
    protected override async Task<HandlerResult<TResponse>> HandleAsync([NotNull] TRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        await AdditionalNullChecks(request).ConfigureAwait(false);

        var ret = await ExecuteService(request, cancellationToken).ConfigureAwait(false);

        return this.ExecutedResult(ret);
    }

    protected async virtual Task AdditionalNullChecks(TRequest request)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}

