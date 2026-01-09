using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchLinkResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string? LinkedServiceTypology { get; set; }
    public string? LinkedServiceName { get; set; }
    public long LinkedServiceId { get; set; }
    public string? VlanId { get; set; }
    public VirtualSwitchLinkStatuses? Status { get; set; }
    public string? VirtualSwitchId { get; set; }
    public string? VirtualSwitchName { get; set; }
}
