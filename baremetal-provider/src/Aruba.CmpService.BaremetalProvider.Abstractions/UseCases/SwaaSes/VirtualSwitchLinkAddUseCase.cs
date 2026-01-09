using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;

[NonTransactional]
public class VirtualSwitchLinkAddUseCase : SwaasBaseUseCase<VirtualSwitchLinkAddUseCaseRequest, VirtualSwitchLinkAddUseCaseResponse, VirtualSwitchLink>
{
    public VirtualSwitchLinkAddUseCase(ISwaasesService swaasesService) :
        base(swaasesService)
    {
    }

    protected override async Task<ServiceResult<VirtualSwitchLink>> ExecuteService(VirtualSwitchLinkAddUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SwaasesService.AddVirtualSwitchLink(request, cancellationToken).ConfigureAwait(false);
    }
}

