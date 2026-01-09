using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal;
public class InternalGetRegionsUseCase : UseCase<InternalGetRegionsUseCaseRequest, InternalGetRegionsUseCaseResponse>
{
    private readonly IInternalService internalService;
    public InternalGetRegionsUseCase(IInternalService internalService)
    {
        this.internalService = internalService;
    }

    protected override async Task<HandlerResult<InternalGetRegionsUseCaseResponse>> HandleAsync([NotNull] InternalGetRegionsUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await internalService.GetRegions(request, cancellationToken).ConfigureAwait(false);

        return this.ExecutedResult(ret);
    }
}
