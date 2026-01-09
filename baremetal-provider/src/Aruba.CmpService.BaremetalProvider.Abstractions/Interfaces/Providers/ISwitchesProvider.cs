using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface ISwitchesProvider :
    ILegacyProvider<LegacySwitchListItem, LegacySwitchDetail>
{
}
