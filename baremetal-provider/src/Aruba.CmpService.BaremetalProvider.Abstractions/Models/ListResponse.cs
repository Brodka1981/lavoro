using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ListResponse<T>
{
    /// <summary>
    /// Total count
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Values
    /// </summary>
    public ICollection<T> Values { get; init; } = new List<T>();
}
