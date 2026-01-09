using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class MCIGetByIdQueryHandler : IQueryHandler<MCIByIdRequest, MCI>
{
    private readonly IMCIsService mciService;

    public MCIGetByIdQueryHandler(IMCIsService mciService)
    {
        this.mciService = mciService;
    }

    public async Task<MCI?> Handle(MCIByIdRequest request)
    {
        ParametersCheck(request);

        var result = new ServiceResult<MCI>();

        if(request.CalculatePrices)
        {
            result = await this.mciService.GetByIdWithPrices(request, CancellationToken.None).ConfigureAwait(false);
        }
        else
        {
            result = await this.mciService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        }
            
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(MCIByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
