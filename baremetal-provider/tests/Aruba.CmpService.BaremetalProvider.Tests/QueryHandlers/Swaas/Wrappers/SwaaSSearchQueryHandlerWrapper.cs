using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS.Wrappers;

internal class SwaasSearchQueryHandlerWrapper : SwaasSearchQueryHandler
{
    public SwaasSearchQueryHandlerWrapper(ISwaasesService swaasesService)
        : base(swaasesService)
    { }

    public async Task<SwaasList> Handle(SwaasSearchFilterRequest request)
    {
        return await base.Handle(request);
    }
}
