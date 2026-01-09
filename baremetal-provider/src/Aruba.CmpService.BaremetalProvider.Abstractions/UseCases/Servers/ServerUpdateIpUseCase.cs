using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;

[NonTransactional]
public class ServerUpdateIpUseCase : ServerNoResponseBaseUseCase<ServerUpdateIpUseCaseRequest, ServerUpdateIpUseCaseResponse>
{
    public ServerUpdateIpUseCase(IServersService serversService) :
        base(serversService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService([NotNull] ServerUpdateIpUseCaseRequest request, CancellationToken cancellationToken)
    {
        var ret = await this.ServersService.UpdateIpAddress(request, cancellationToken).ConfigureAwait(false);
        return ret;
    }
}
