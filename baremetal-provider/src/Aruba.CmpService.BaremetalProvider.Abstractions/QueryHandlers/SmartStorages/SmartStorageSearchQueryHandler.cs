using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class SmartStorageSearchQueryHandler : IQueryHandler<SmartStorageSearchFilterRequest, SmartStorageList>
{
    private readonly ISmartStoragesService smartStorageService;

    public SmartStorageSearchQueryHandler(ISmartStoragesService smartStorageService)
    {
        this.smartStorageService = smartStorageService;
    }

    public async Task<SmartStorageList> Handle(SmartStorageSearchFilterRequest request)
    {
        ParametersCheck(request);

        var smartStorages = await smartStorageService.Search(request, CancellationToken.None).ConfigureAwait(false);
        return smartStorages.Value!;
    }

    private static void ParametersCheck(SmartStorageSearchFilterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
