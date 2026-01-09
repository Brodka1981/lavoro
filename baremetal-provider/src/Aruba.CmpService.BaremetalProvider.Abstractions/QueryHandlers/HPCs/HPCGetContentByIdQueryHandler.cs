using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class HPCGetContentByIdQueryHandler : IQueryHandler<HPCContentByIdRequest, HPC>
{
    private readonly IHPCsService hpcService;

    public HPCGetContentByIdQueryHandler(IHPCsService hpcService)
    {
        this.hpcService = hpcService;
    }

    public async Task<HPC?> Handle(HPCContentByIdRequest request)
    {
        ParametersCheck(request);

        var result = await this.hpcService.GetContentById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(HPCByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
