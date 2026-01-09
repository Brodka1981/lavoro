using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class HPCGetByIdQueryHandler : IQueryHandler<HPCByIdRequest, HPC>
{
    private readonly IHPCsService hpcService;

    public HPCGetByIdQueryHandler(IHPCsService hpcService)
    {
        this.hpcService = hpcService;
    }

    public async Task<HPC?> Handle(HPCByIdRequest request)
    {
        ParametersCheck(request);

        var result = new ServiceResult<HPC>();

        if (request.CalculatePrices)
        {
            result = await this.hpcService.GetByIdWithPrices(request, CancellationToken.None).ConfigureAwait(false);
        }
        else
        {
            result = await this.hpcService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        }

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
