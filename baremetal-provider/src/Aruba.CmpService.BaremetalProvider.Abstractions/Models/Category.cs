using System.Diagnostics.CodeAnalysis;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Category
{
    /// <summary>
    /// Category name
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Provider
    /// </summary>
    public string? Provider { get; set; }
    
    /// <summary>
    /// Typology
    /// </summary>
    public Typology? Typology { get; set; }
}
