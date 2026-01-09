using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;

[NonTransactional]
public class VirtualSwitchAddUseCase : SwaasBaseUseCase<VirtualSwitchAddUseCaseRequest, VirtualSwitchAddUseCaseResponse, VirtualSwitch>
{
    public VirtualSwitchAddUseCase(ISwaasesService swaasesService) :
        base(swaasesService)
    {
    }

    protected override async Task<ServiceResult<VirtualSwitch>> ExecuteService(VirtualSwitchAddUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SwaasesService.AddVirtualSwitch(request, cancellationToken).ConfigureAwait(false);
    }
}
