using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class HPCSearchQueryHandler : IQueryHandler<HPCSearchFilterRequest, HPCList>
{
    private readonly IHPCsService hpcService;

    public HPCSearchQueryHandler(IHPCsService hpcService)
    {
        this.hpcService = hpcService;
    }

    public async Task<HPCList> Handle(HPCSearchFilterRequest request)
    {
        ParametersCheck(request);

        var hpcs = await this.hpcService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return hpcs.Value!;
    }

    private static void ParametersCheck(HPCSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
