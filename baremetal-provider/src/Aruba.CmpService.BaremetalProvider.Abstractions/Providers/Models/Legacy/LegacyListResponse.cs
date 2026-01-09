using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyListResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Offset { get; set; }
    public int TotalItems { get; set; }
}
