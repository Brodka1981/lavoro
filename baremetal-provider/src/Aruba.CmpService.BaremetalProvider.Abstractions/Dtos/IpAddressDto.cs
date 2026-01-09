using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class IpAddressDto
{
    public string? Description { get; set; }
    public ICollection<string> HostNames { get; init; } = new Collection<string>();
}
