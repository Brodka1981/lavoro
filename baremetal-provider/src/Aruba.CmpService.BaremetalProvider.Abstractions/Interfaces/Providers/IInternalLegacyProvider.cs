using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface IInternalLegacyProvider
{
    /// <summary>
    /// Set autorenew for a resource list
    /// </summary>
    Task<ApiCallOutput<bool>> UpsertAutomaticRenewAsync(LegacyAutoRenew body);

    /// <summary>
    /// Get resources
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetLegacyResources();

    /// <summary>
    /// Get resources by ids
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderServices(List<LegacyResourceFilter> services, bool getPrices);

    /// <summary>
    /// Get autorecharge info
    /// </summary>
    Task<ApiCallOutput<LegacyAutorechargeData>> GetAutorechargeAsync();

    /// <summary>
    /// Get regions
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyRegion>>> GetRegions();

    /// <summary>
    /// Get legacy services with related region
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>> GetServicesWithRegions();
}
