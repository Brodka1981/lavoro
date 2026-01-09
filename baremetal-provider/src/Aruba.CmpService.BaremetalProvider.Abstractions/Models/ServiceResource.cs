using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServiceResource
{
    /// <summary>
    /// Service resource path
    /// </summary>
    public string? Path { get; set; }
}
