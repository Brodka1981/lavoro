using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class UpdateIpAddress
{
    public long IpAddressId { get; set; }
    public string? CustomName { get; set; }
    public IEnumerable<string> Hosts { get; set; } = new List<string>();
}
