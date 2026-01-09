using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;
public class SmartStorageSnapshotsQueryHandler : IQueryHandler<SmartStorageSnapshotsRequest, SmartStorageSnapshots>
{
    private readonly ISmartStoragesService smartStoragesService;

    public SmartStorageSnapshotsQueryHandler(ISmartStoragesService smartStoragesService)

    {
        this.smartStoragesService = smartStoragesService;
    }

    public async Task<SmartStorageSnapshots?> Handle(SmartStorageSnapshotsRequest request)
    {
        ParametersCheck(request);

        var result = await smartStoragesService.GetSnapshots(request).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SmartStorageSnapshotsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
