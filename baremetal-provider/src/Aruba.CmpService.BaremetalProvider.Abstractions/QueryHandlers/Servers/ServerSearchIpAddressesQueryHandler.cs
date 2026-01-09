using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers;

public class ServerSearchIpAddressesQueryHandler :
    IQueryHandler<ServerIpAddressesFilterRequest, ServerIpAddressList>
{
    private readonly IServersService serversService;

    public ServerSearchIpAddressesQueryHandler(IServersService serversService)
    {
        this.serversService = serversService;
    }

    public async Task<ServerIpAddressList> Handle(ServerIpAddressesFilterRequest request)
    {
        request.ThrowIfNull();

        var servers = await serversService.SearchIpAddresses(request, CancellationToken.None).ConfigureAwait(false);

        if (!servers.Errors.Any())
        {
            return servers.Value;
        }
        return null;
    }
}
