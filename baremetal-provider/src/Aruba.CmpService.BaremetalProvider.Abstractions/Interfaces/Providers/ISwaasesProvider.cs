using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

public interface ISwaasesProvider :
    ILegacyProvider<LegacySwaasListItem, LegacySwaasDetail>
{
    Task<ApiCallOutput<List<LegacyVirtualSwitch>>> GetVirtualSwitches(string swaasId);
    Task<ApiCallOutput<List<LegacyVirtualSwitchLink>>> GetVirtualSwitchLinks(string swaasId);
    Task<ApiCallOutput<LegacyVirtualSwitch>> AddVirtualSwitch(AddLegacyVirtualSwitch virtualSwitch);
    Task<ApiCallOutput<LegacyVirtualSwitch>> EditVirtualSwitch(EditLegacyVirtualSwitch virtualSwitch);
    Task<ApiCallOutput> DeleteVirtualSwitch(DeleteLegacyVirtualSwitch virtualSwitch);
    Task<ApiCallOutput> AddVirtualSwitchLink(string swaasId, string virtualSwitchId, long serviceId, LegacyServiceType serviceType);
    Task<ApiCallOutput> DeleteVirtualSwitchLink(string swaasId, string virtualSwitchId, string virtualSwitchLinkId);
}
