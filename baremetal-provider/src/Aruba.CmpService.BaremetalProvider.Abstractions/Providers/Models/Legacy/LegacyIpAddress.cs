using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyIpAddress
{
    public long IpAddressId { get; set; }
    public string? IpAddress { get; set; }
    public string? CustomName { get; set; }
    public LegacyIpAddressStatuses Status { get; set; }
    public int IpType { get; set; }
    public IEnumerable<string> Hosts { get; set; } = new List<string>();
}
