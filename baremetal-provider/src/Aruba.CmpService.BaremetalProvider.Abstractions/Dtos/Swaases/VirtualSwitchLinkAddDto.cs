using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchLinkAddDto
{
    public string? LinkedServiceTypology { get; set; }
    public long? LinkedServiceId { get; set; }
    public string? VirtualSwitchId { get; set; }
}
