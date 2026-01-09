using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases.Responses;
using Aruba.MessageBus.Transactions;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;

[NonTransactional]
public class SwaasRenameUseCase : SwaasNoResponseBaseUseCase<SwaasRenameUseCaseRequest, SwaasRenameUseCaseResponse>
{
    public SwaasRenameUseCase(ISwaasesService swaasesService)
        : base(swaasesService)
    { }

    protected override async Task<ServiceResult> ExecuteService(SwaasRenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await SwaasesService.Rename(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
