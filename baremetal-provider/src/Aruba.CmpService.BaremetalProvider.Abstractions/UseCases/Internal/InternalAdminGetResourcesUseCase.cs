using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;

[NonTransactional]
public class InternalAdminGetResourcesUseCase : UseCase<InternalAdminGetResourcesUseCaseRequest, InternalAdminGetResourcesUseCaseResponse>
{
    private readonly IInternalService internalService;

    public InternalAdminGetResourcesUseCase(IInternalService internalService)
    {
        this.internalService = internalService;
    }

    protected override async Task<HandlerResult<InternalAdminGetResourcesUseCaseResponse>> HandleAsync([NotNull] InternalAdminGetResourcesUseCaseRequest request, CancellationToken cancellationToken)
    {
        ParametersCheck(request);

        var result = await this.internalService.AdminGetAllResources(request, CancellationToken.None).ConfigureAwait(false);

        return this.ExecutedResult(result);
    }

    private static void ParametersCheck(InternalAdminGetResourcesUseCaseRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
