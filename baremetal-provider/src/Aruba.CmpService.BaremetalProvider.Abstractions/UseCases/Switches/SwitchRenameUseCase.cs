using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Switches;

[NonTransactional]
public class SwitchRenameUseCase : SwitchNoResponseBaseUseCase<SwitchRenameUseCaseRequest, SwitchRenameUseCaseResponse>
{
    public SwitchRenameUseCase(ISwitchesService switchesService)
        : base(switchesService)
    { }

    protected override async Task<ServiceResult> ExecuteService(SwitchRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await this.SwitchesService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
