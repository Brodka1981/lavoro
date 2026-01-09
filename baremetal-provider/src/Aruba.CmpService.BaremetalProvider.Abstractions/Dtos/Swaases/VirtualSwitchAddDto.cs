using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchAddDto :
    VirtualSwitchEditDto
{
    public string? Location { get; set; }
}
