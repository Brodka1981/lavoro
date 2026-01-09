using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class DisableReasonDetail
{
    /// <summary>
    /// Reason
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }
    
    /// <summary>
    /// Notes
    /// </summary>
    public string? Note { get; set; }
    
    /// <summary>
    /// Disable 
    /// </summary>
    public DisableMode Mode { get; set; } = DisableMode.Automatic;
}