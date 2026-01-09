//using System.Diagnostics.CodeAnalysis;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
//using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
//using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
//using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
//using Aruba.MessageBus.UseCases;
//using Throw;

//namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches;
//public abstract class SwitchBaseUseCase<TRequest, TResponse> : UseCase<TRequest, TResponse>
//    where TRequest : BaseUseCaseRequest
//    where TResponse : MessageBusResponse<Switch>
//{
//    protected ISwitchesService SwitchesService { get; }

//    protected SwitchBaseUseCase(ISwitchesService switchesService)
//    {
//        SwitchesService = switchesService;
//    }
//    protected override async Task<HandlerResult<TResponse>> HandleAsync([NotNull] TRequest request, CancellationToken cancellationToken)
//    {
//        request.ThrowIfNull();
//        request.ResourceId.ThrowIfNull();

//        await AdditionalNullChecks(request).ConfigureAwait(false);

//        var ret = await ExecuteService(request, cancellationToken).ConfigureAwait(false);

//        return this.ExecutedResult(ret);
//    }

//    protected abstract Task<ServiceResult<Switch>> ExecuteService(TRequest request, CancellationToken cancellationToken);

//    protected async virtual Task AdditionalNullChecks(TRequest request)
//    {
//        await Task.CompletedTask.ConfigureAwait(false);
//    }
//}

