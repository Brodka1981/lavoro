using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs;

[NonTransactional]
public class MCIRenameUseCase : MCINoResponseBaseUseCase<MCIRenameUseCaseRequest, MCIRenameUseCaseResponse>
{
    public MCIRenameUseCase(IMCIsService mcisService)
        : base(mcisService)
    { }

    protected override async Task<ServiceResult> ExecuteService(MCIRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await MCIsService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
