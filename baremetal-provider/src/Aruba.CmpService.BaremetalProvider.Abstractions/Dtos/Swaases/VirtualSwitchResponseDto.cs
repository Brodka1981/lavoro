using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public VirtualSwitchStatuses? Status { get; set; }
    public IEnumerable<string> LocationCodes { get; set; } = new List<string>();
}
