using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;

public class ServerSetAutomaticRenewUseCase : ServerNoResponseBaseUseCase<ServerSetAutomaticRenewUseCaseRequest, ServerSetAutomaticRenewUseCaseResponse>
{
    public ServerSetAutomaticRenewUseCase(IServersService ServersService)
    : base(ServersService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(ServerSetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await ServersService.SetAutomaticRenew(request, cancellationToken).ConfigureAwait(false);

        return ret;
    }
}
