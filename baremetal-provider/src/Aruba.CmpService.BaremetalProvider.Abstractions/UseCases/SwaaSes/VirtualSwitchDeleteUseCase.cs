using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;

[NonTransactional]
public class VirtualSwitchDeleteUseCase : SwaasNoResponseBaseUseCase<VirtualSwitchDeleteUseCaseRequest, VirtualSwitchDeleteUseCaseResponse>
{
    public VirtualSwitchDeleteUseCase(ISwaasesService swaasesService) :
        base(swaasesService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(VirtualSwitchDeleteUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SwaasesService.DeleteVirtualSwitch(request, cancellationToken).ConfigureAwait(false);
    }
}
