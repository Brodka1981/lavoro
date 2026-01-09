using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Responses;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class GetAvailableSmartFoldersQueryHandler :
    IQueryHandler<GetAvailableSmartFoldersRequest, GetAvailableSmartFoldersResponse>
{
    private readonly ISmartStoragesService smartStoragesService;

    public GetAvailableSmartFoldersQueryHandler(ISmartStoragesService smartStoragesService)
    {
        this.smartStoragesService = smartStoragesService;
    }

    public async Task<GetAvailableSmartFoldersResponse?> Handle(GetAvailableSmartFoldersRequest request)
    {
        var result = await smartStoragesService.GetAvailableSmartFolders(request).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }
}