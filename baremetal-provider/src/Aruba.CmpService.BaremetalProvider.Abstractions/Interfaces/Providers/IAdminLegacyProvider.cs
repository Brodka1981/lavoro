using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface IAdminLegacyProvider
{
    /// <summary>
    /// Get resources by ids
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderServices(List<LegacyResourceFilter> services, string userId);

    /// <summary>
    /// Get resources
    /// </summary>
    Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderLinkableServices(string userId);

    /// <summary>
    /// Get autorecharge info
    /// </summary>
    Task<ApiCallOutput<LegacyAutorechargeData>> GetAutorechargeAsync(string userId);
}
