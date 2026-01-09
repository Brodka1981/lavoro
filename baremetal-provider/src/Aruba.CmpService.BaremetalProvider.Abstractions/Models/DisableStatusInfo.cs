using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class DisableStatusInfo
{
    /// <summary>
    /// True if Disabled
    /// </summary>
    public bool IsDisabled { get; set; }
    
    /// <summary>
    /// Disable reasons 
    /// </summary>
    public Collection<DisableReasonDetail> Reasons { get; init; } = new Collection<DisableReasonDetail>();

    /// <summary>
    /// Previous status
    /// </summary>
    public PreviousStatus? PreviousStatus { get; set; }
}
