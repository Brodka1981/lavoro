using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;

[NonTransactional]
public class InternalGetResourcesUseCase : UseCase<InternalGetResourcesUseCaseRequest, InternalGetResourcesUseCaseResponse>
{
    private readonly IInternalService internalService;

    public InternalGetResourcesUseCase(IInternalService internalService)
    {
        this.internalService = internalService;
    }

    protected override async Task<HandlerResult<InternalGetResourcesUseCaseResponse>> HandleAsync([NotNull] InternalGetResourcesUseCaseRequest request, CancellationToken cancellationToken)
    {
        ParametersCheck(request);

        var result = await this.internalService.GetAllResources(request, CancellationToken.None).ConfigureAwait(false);

        return this.ExecutedResult(result);
    }

    private static void ParametersCheck(InternalGetResourcesUseCaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
