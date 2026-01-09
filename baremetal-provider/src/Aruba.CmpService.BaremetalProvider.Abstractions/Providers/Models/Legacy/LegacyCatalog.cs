using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyCatalog
{
    public string? Code { get; set; }
    public IEnumerable<LegacyCatalogItem> Elements { get; set; } = new List<LegacyCatalogItem>();
}
