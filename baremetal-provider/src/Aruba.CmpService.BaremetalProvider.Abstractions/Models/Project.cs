using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Project
{
    /// <summary>
    /// Project id
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Project name
    /// </summary>
    public string? Name { get; set; }
}
