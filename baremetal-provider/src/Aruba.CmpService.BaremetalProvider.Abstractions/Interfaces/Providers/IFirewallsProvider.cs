using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface IFirewallsProvider :
    ILegacyProvider<LegacyFirewallListItem, LegacyFirewallDetail>
{
    Task<ApiCallOutput<LegacyListResponse<LegacyIpAddress>>> SearchIpAddresses(LegacySearchFilters filterRequest, long firewallId);
    Task<ApiCallOutput<LegacyListResponse<LegacyVlanId>>> GetVlanIds(LegacySearchFilters filterRequest, long firewallId);
    Task<ApiCallOutput<bool>> UpdateIpAddress(UpdateIpAddress updateIp);

}
