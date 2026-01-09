using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Tests.QueryHandlers.SwaaS.Wrappers;

internal class SwaaSGetByIdQueryHandlerWrapper : SwaasGetByIdQueryHandler
{
    public SwaaSGetByIdQueryHandlerWrapper(
        ISwaasesService swaasService)
        : base(swaasService)
    { }

    public async Task<Swaas?> Handle(SwaasByIdRequest request)
    {
        return await base.Handle(request).ConfigureAwait(false);
    }
}
