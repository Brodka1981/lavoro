using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;

[NonTransactional]
public class ServerRestartUseCase : ServerNoResponseBaseUseCase<ServerRestartUseCaseRequest, ServerRestartUseCaseResponse>
{
    public ServerRestartUseCase(IServersService serversService) :
        base(serversService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(ServerRestartUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await this.ServersService.Restart(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}