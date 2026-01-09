using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Responses;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.UseCases;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs;

// FIXME: @matteo.filippa check
[NonTransactional]
public class HPCCreateUseCase : UseCase<HPCCreateUseCaseRequest, HPCCreateUseCaseResponse>
{
    private IHPCsService hpcsService;

    public HPCCreateUseCase(IHPCsService hpcsService)
        : base()
    {
        this.hpcsService = hpcsService;
    }

    protected override async Task<HandlerResult<HPCCreateUseCaseResponse>> HandleAsync([NotNull] HPCCreateUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        var ret = await ExecuteService(request, cancellationToken).ConfigureAwait(false);

        return this.ExecutedResult(ret);
    }

    private async Task<ServiceResult<HPC>> ExecuteService(HPCCreateUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await hpcsService.Create(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
