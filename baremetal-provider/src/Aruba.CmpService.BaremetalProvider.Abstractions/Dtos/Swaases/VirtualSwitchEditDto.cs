using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchEditDto
{
    public string? Name { get; set; }
}
