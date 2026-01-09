using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;
public class SmartStorageSearchFoldersQueryHandler : IQueryHandler<SmartStorageFoldersRequest, IEnumerable<SmartStorageFoldersItem>>
{
    private readonly ISmartStoragesService smartStoragesService;

    public SmartStorageSearchFoldersQueryHandler(ISmartStoragesService smartStoragesService)
    {
        this.smartStoragesService = smartStoragesService;
    }
    public async Task<IEnumerable<SmartStorageFoldersItem>> Handle(SmartStorageFoldersRequest request)
    {
        ParametersCheck(request);
        var folders = await smartStoragesService.SearchFolders(request, CancellationToken.None).ConfigureAwait(false);
        if (!folders.Errors.Any())
        {
            return folders.Value!;
        }
        return null!;
    }
    private static void ParametersCheck(SmartStorageFoldersRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
