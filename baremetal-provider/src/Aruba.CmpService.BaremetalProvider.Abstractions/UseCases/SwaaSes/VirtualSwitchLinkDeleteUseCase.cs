using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;

[NonTransactional]
public class VirtualSwitchLinkDeleteUseCase : SwaasNoResponseBaseUseCase<VirtualSwitchLinkDeleteUseCaseRequest, VirtualSwitchLinkDeleteUseCaseResponse>
{
    public VirtualSwitchLinkDeleteUseCase(ISwaasesService swaasesService) :
        base(swaasesService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(VirtualSwitchLinkDeleteUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SwaasesService.DeleteVirtualSwitchLink(request, cancellationToken).ConfigureAwait(false);
    }
}
