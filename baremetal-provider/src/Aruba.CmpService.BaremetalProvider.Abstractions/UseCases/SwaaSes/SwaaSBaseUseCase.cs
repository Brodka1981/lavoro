using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.MessageBus.UseCases;
using Throw;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;
public abstract class SwaasBaseUseCase<TRequest, TResponse> : SwaasBaseUseCase<TRequest, TResponse, Swaas>
    where TRequest : BaseUseCaseRequest
    where TResponse : MessageBusResponse<Swaas>
{
    protected SwaasBaseUseCase(ISwaasesService swaaSesService) :
        base(swaaSesService)
    {
    }

    protected async virtual Task AdditionalNullChecks(TRequest request)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}

public abstract class SwaasBaseUseCase<TRequest, TResponse, TTypeResponse> : UseCase<TRequest, TResponse>
    where TRequest : BaseUseCaseRequest
    where TResponse : MessageBusResponse<TTypeResponse>
    where TTypeResponse : class, new()
{
    protected ISwaasesService SwaasesService { get; }

    protected SwaasBaseUseCase(ISwaasesService swaaSesService)
    {
        SwaasesService = swaaSesService;
    }
    protected override async Task<HandlerResult<TResponse>> HandleAsync([NotNull] TRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        await AdditionalNullChecks(request).ConfigureAwait(false);

        var ret = await ExecuteService(request, cancellationToken).ConfigureAwait(false);

        return this.ExecutedResult(ret);
    }

    protected abstract Task<ServiceResult<TTypeResponse>> ExecuteService(TRequest request, CancellationToken cancellationToken);

    protected async virtual Task AdditionalNullChecks(TRequest request)
    {
        await Task.CompletedTask.ConfigureAwait(false);
    }
}


