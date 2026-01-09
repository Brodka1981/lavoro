using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Certificate
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }
    
    /// <summary>
    /// Cartificate
    /// </summary>
    public string Cert { get; set; }

    public Certificate()
    {
        Key = string.Empty;
        Cert = string.Empty;
    }
}
