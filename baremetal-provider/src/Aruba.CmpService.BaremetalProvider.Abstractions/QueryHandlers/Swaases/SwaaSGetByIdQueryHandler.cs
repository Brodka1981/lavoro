using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;

public class SwaasGetByIdQueryHandler : IQueryHandler<SwaasByIdRequest, Swaas>
{
    private readonly ISwaasesService swaasesService;

    public SwaasGetByIdQueryHandler(ISwaasesService swaasesService)
    {
        this.swaasesService = swaasesService;
    }

    public async Task<Swaas?> Handle(SwaasByIdRequest request)
    {
        ParametersCheck(request);

        var result = await swaasesService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SwaasByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
