using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface IServersProvider :
    ILegacyProvider<LegacyServerListItem, LegacyServerDetail>
{
    Task<ApiCallOutput<bool>> Restart(long id);
    Task<ApiCallOutput<LegacyListResponse<LegacyIpAddress>>> SearchIpAddresses(LegacySearchFilters filterRequest, long serverId);
    Task<ApiCallOutput<bool>> UpdateIpAddress(UpdateIpAddress updateIp);
    Task<ApiCallOutput<bool>> DeletePleskLicense(DeletePleskLicense deletePleskLicense);
}
