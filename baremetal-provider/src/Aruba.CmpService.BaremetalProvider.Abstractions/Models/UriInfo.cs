using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class UriInfo
{
    /// <summary>
    /// True if is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Resource id
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// Project id
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Resource type
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// Resource provider
    /// </summary>
    public string? ResourceProvider { get; set; }
}
