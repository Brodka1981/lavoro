using System.Collections.ObjectModel;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

public interface IResourceBase
{
    /// <summary>
    /// Category
    /// </summary>
    Category? Category { get; set; }

    /// <summary>
    /// CreationDate 
    /// </summary>
    DateTimeOffset? CreationDate { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    string? Id { get; set; }

    /// <summary>
    /// Uri
    /// </summary>
    string? Uri { get; set; }

    /// <summary>
    /// Update date
    /// </summary>
    DateTimeOffset? UpdateDate { get; set; }

    /// <summary>
    /// Location
    /// </summary>
    Location? Location { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    string? Name { get; set; }

    /// <summary>
    /// Project
    /// </summary>
    Project? Project { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    Status? Status { get; set; }

    /// <summary>
    /// Ecommerce
    /// </summary>
    Ecommerce? Ecommerce { get; set; }

    /// <summary>
    /// Tags
    /// </summary>
    Collection<string>? Tags { get; init; }

    /// <summary>
    /// Version
    /// </summary>
    Version? Version { get; set; }

    /// <summary>
    /// Creation by user
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// True if Has Different Project From
    /// </summary>
    bool HasDifferentProjectFrom(IResourceBase resourceBase);

    /// <summary>
    /// Linked resource
    /// </summary>
    Collection<LinkedResource> LinkedResources { get; set; }
}
