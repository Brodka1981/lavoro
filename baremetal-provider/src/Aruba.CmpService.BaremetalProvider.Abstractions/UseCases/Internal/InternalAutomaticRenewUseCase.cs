using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;
public class InternalAutomaticRenewUseCase : NoResponseBaseUseCase<InternalAutomaticRenewUseCaseRequest, InternalAutomaticRenewUseCaseResponse>
{
    private readonly IInternalService internalService;

    public InternalAutomaticRenewUseCase(IInternalService internalService)
    {
        this.internalService = internalService;
    }

    protected override async Task<ServiceResult> ExecuteService(InternalAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await internalService.UpsertAutomaticRenew(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}