using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Shared;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LinkedResource
{
    /// <summary>
    /// Id
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Typology
    /// </summary>
    public string? Typology { get; set; }

    /// <summary>
    /// Uri
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// Link Creation Date
    /// </summary>
    public DateTimeOffset? LinkCreationDate { get; set; }

    /// <summary>
    /// Link type
    /// </summary>
    public LinkedResourceType LinkType { get; set; } = LinkedResourceType.Use;

    /// <summary>
    /// Relation Name 
    /// </summary>
    public string? RelationName { get; set; }

    /// <summary>
    /// True if ready
    /// </summary>
    public bool Ready { get; set; }

    /// <summary>
    /// True if strict correlation
    /// </summary>
    public bool StrictCorrelation { get; set; }
}
