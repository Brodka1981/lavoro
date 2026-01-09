using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;

public class MCIGetContentByIdQueryHandler : IQueryHandler<MCIContentByIdRequest, MCI>
{
    private readonly IMCIsService mciService;

    public MCIGetContentByIdQueryHandler(IMCIsService mciService)
    {
        this.mciService = mciService;
    }

    public async Task<MCI?> Handle(MCIContentByIdRequest request)
    {
        ParametersCheck(request);

        var result = await this.mciService.GetContentById(request, CancellationToken.None).ConfigureAwait(false);
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
