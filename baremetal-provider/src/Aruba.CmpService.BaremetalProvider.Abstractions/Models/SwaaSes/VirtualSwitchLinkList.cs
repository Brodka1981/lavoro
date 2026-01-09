using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]

public class VirtualSwitchLinkList : ListResponse<VirtualSwitchLink>
{
}
