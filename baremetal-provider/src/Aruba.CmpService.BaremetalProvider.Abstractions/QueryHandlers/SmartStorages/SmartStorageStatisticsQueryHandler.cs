using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages;

public class SmartStorageStatisticsQueryHandler : IQueryHandler<SmartStorageStatisticsRequest, SmartStorageStatistics>
{
    private readonly ISmartStoragesService smartStoragesService;

    public SmartStorageStatisticsQueryHandler(ISmartStoragesService swaasesService)
    {
        smartStoragesService = swaasesService;
    }

    public async Task<SmartStorageStatistics> Handle(SmartStorageStatisticsRequest request)
    {
        ParametersCheck(request);

        var result = await smartStoragesService.GetStatistics(request).ConfigureAwait(false);
        if (!result.Errors.Any())
        {
            return result.Value;
        }
        return null;
    }

    private static void ParametersCheck(SmartStorageStatisticsRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
    }
}
