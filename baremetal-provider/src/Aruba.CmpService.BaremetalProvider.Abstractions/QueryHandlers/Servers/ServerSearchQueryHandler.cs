using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers;

public class ServerSearchQueryHandler : IQueryHandler<ServerSearchFilterRequest, ServerList>
{
    private readonly IServersService serversService;

    public ServerSearchQueryHandler(IServersService serversService)
    {
        this.serversService = serversService;
    }

    public async Task<ServerList> Handle(ServerSearchFilterRequest request)
    {
        ParametersCheck(request);

        var servers = await serversService.Search(request, CancellationToken.None).ConfigureAwait(false);
        if (!servers.Errors.Any())
        {
            return servers.Value;
        }
        return null;
    }

    private static void ParametersCheck(ServerSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
