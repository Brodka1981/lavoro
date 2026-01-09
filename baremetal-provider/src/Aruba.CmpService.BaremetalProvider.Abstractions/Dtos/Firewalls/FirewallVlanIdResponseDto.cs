using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Firewalls;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class FirewallVlanIdResponseDto
{
    public string? Vlanid { get; set; }
}
