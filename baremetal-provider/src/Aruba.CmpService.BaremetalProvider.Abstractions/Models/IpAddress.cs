using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class IpAddress
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Ip
    /// </summary>
    public string? Ip { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public IpAddressTypes Type { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public IpAddressStatuses? Status { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// HostNames
    /// </summary>
    public IEnumerable<string> HostNames { get; set; } = new List<string>();
}
