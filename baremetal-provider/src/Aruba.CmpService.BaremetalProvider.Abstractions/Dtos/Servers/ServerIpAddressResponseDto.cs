using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerIpAddressResponseDto
{
    public long Id { get; set; }
    public string? Ip { get; set; }
    public IpAddressTypes Type { get; set; }
    public string? Description { get; set; }
    public IpAddressStatuses? Status { get; set; }
    public ICollection<string> HostNames { get; init; } = new Collection<string>();
}
