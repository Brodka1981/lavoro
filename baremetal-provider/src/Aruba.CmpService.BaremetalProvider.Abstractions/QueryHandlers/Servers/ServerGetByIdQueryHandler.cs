using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers;
public class ServerGetByIdQueryHandler : IQueryHandler<ServerByIdRequest, Server>
{
    private readonly IServersService serversService;

    public ServerGetByIdQueryHandler(IServersService serversService)
    {
        this.serversService = serversService;
    }

    public async Task<Server?> Handle(ServerByIdRequest request)
    {
        ParametersCheck(request);

        var result = await serversService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(ServerByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
