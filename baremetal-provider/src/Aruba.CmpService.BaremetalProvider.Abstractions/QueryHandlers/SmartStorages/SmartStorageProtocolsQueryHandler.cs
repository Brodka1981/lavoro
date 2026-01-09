using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class SmartStorageProtocolsQueryHandler : IQueryHandler<SmartStorageProtocolsRequest, IEnumerable<SmartStorageProtocol>>
{
    private readonly ISmartStoragesService smartStoragesService;

    public SmartStorageProtocolsQueryHandler(ISmartStoragesService swaasesService)
    {
        smartStoragesService = swaasesService;
    }

    public async Task<IEnumerable<SmartStorageProtocol>> Handle(SmartStorageProtocolsRequest request)
    {
        ParametersCheck(request);

        var result = await smartStoragesService.GetProtocols(request).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SmartStorageProtocolsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
