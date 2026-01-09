using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class DataVersion
{
    public DataVersion()
    {
        Current = 1;
    }
    
    /// <summary>
    /// Current version
    /// </summary>
    public int Current { get; set; }
    
    /// <summary>
    /// Previous version
    /// </summary>
    public int? Previous { get; set; }
}
