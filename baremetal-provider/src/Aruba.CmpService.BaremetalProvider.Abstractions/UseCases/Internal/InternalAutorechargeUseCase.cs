using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;

[NonTransactional]
public class InternalAutorechargeUseCase : UseCase<InternalAutorechargeUseCaseRequest, InternalAutorechargeUseCaseResponse>
{
    private readonly IInternalService internalService;
    public InternalAutorechargeUseCase(IInternalService internalService)
    {
        this.internalService = internalService;
    }

    protected override async Task<HandlerResult<InternalAutorechargeUseCaseResponse>> HandleAsync([NotNull] InternalAutorechargeUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await internalService.GetAutorecharge(request, cancellationToken).ConfigureAwait(false);

        return this.ExecutedResult(ret);
    }
}
