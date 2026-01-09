using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class SmartStorageGetByIdQueryHandler : IQueryHandler<SmartStorageByIdRequest, SmartStorage>
{
    private readonly ISmartStoragesService smartStorageService;

    public SmartStorageGetByIdQueryHandler(ISmartStoragesService smartStorageService)
    {
        this.smartStorageService = smartStorageService;
    }

    public async Task<SmartStorage?> Handle(SmartStorageByIdRequest request)
    {
        ParametersCheck(request);

        var result = await smartStorageService.GetById(request, CancellationToken.None).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SmartStorageByIdRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
