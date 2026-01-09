using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LocationMap
{
    /// <summary>
    /// Id
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Value
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Legacy value 
    /// </summary>
    public string? LegacyValue { get; set; }
}
