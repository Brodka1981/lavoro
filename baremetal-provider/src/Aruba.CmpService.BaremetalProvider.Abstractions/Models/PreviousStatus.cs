using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class PreviousStatus
{
    /// <summary>
    /// State
    /// </summary>
    public string? State { get; set; }
    
    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }
}
