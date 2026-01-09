using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.MCIs.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.MCIs;

public class MCIRenameUseCaseWrapper :
    MCIRenameUseCase
{
    public MCIRenameUseCaseWrapper(IMCIsService mcisService) :
        base(mcisService)
    {

    }
    public async Task<ServiceResult> Execute(MCIRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}
public class MCISetAutomaticRenewUseCaseWrapper :
    MCISetAutomaticRenewUseCase
{
    public MCISetAutomaticRenewUseCaseWrapper(IMCIsService mcisService) :
        base(mcisService)
    {

    }
    public async Task<ServiceResult> Execute(MCISetAutomaticRenewUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}