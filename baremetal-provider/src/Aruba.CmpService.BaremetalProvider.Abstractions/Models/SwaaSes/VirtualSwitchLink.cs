using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class VirtualSwitchLink
{
    public string? Id { get; set; }
    public long LinkedServiceId { get; set; }
    public string? LinkedServiceTypology { get; set; }
    public string? LinkedServiceName { get; set; }
    public string? VlanId { get; set; }
    public string? VirtualSwitchId { get; set; }
    public string? VirtualSwitchName { get; set; }
    public VirtualSwitchLinkStatuses? Status { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
}
