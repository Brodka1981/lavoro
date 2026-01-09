using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class VlanId
{
    /// <summary>
    /// Vlanid
    /// </summary>
    public string? Vlanid { get; set; }
}
