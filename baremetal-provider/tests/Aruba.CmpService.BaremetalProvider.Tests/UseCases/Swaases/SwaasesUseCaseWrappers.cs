using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Swaases;

public class SwaasRenameUseCaseWrapper :
    SwaasRenameUseCase
{
    public SwaasRenameUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(SwaasRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SwaasSetAutomaticRenewUseCaseWrapper :
    SwaasSetAutomaticRenewUseCase
{
    public SwaasSetAutomaticRenewUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(SwaasSetAutomaticRenewUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class VirtualSwitchAddUseCaseWrapper :
    VirtualSwitchAddUseCase
{
    public VirtualSwitchAddUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(VirtualSwitchAddUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class VirtualSwitchDeleteUseCaseWrapper :
    VirtualSwitchDeleteUseCase
{
    public VirtualSwitchDeleteUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(VirtualSwitchDeleteUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class VirtualSwitchEditUseCaseWrapper :
    VirtualSwitchEditUseCase
{
    public VirtualSwitchEditUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(VirtualSwitchEditUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class VirtualSwitchLinkAddUseCaseWrapper :
    VirtualSwitchLinkAddUseCase
{
    public VirtualSwitchLinkAddUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(VirtualSwitchLinkAddUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class VirtualSwitchLinkDeleteUseCaseWrapper :
    VirtualSwitchLinkDeleteUseCase
{
    public VirtualSwitchLinkDeleteUseCaseWrapper(ISwaasesService swaasesService) :
        base(swaasesService)
    {

    }
    public async Task<ServiceResult> Execute(VirtualSwitchLinkDeleteUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}
