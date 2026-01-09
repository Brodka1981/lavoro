using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public partial class ResourceBase<TProperties> : IResourceBase
{
    public ResourceBase()
    {
        Tags = new Collection<string>();
        LinkedResources = new Collection<LinkedResource>();
    }

    /// <summary>
    /// Id
    /// </summary>
    public string? Id { get; set; }
    
    /// <summary>
    /// Uri
    /// </summary>
    public string? Uri { get; set; }
    
    /// <summary>
    /// Name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Location
    /// </summary>
    public Location? Location { get; set; }

    /// <summary>
    /// Project
    /// </summary>
    public Project? Project { get; set; }

    /// <summary>
    /// Tags
    /// </summary>
    public Collection<string>? Tags { get; init; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }

    /// <summary>
    /// Update date
    /// </summary>
    public DateTimeOffset? UpdateDate { get; set; }

    /// <summary>
    /// Created by user
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Updated by user
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public Status? Status { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Ecommerce data
    /// </summary>
    public Ecommerce? Ecommerce { get; set; }

    /// <summary>
    /// Version
    /// </summary>
    public Version? Version { get; set; }

    /// <summary>
    /// Linked resources
    /// </summary>
    public Collection<LinkedResource> LinkedResources { get; set; }

    /// <summary>
    /// Properties
    /// </summary>
    public TProperties? Properties { get; set; }

    /// <summary>
    /// Change version
    /// </summary>
    public void ChangeVersion()
    {
        if (Version is null)
            Version = new Version();

        if (Version.Data is null)
            Version.Data = new DataVersion();

        Version.Data.Previous = Version.Data.Current;
        Version.Data.Current++;
    }

    /// <summary>
    /// True if has different project from
    /// </summary>
    public bool HasDifferentProjectFrom(IResourceBase resourceBase)
    {
        if (resourceBase is null)
            return false;

        if (Project is null && resourceBase.Project != null)
            return true;

        if (Project != null && resourceBase.Project is null)
            return true;

        return Project!.Id != resourceBase.Project!.Id;
    }
}
