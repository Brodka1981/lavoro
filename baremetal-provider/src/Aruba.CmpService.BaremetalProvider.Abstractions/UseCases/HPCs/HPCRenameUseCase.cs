using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs;

[NonTransactional]
public class HPCRenameUseCase : HPCNoResponseBaseUseCase<HPCRenameUseCaseRequest, HPCRenameUseCaseResponse>
{
    public HPCRenameUseCase(IHPCsService mcisService)
        : base(mcisService)
    { }

    protected override async Task<ServiceResult> ExecuteService(HPCRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await HPCsService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
