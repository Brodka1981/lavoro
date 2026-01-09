using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyCatalogItemConfigProduct
{
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public long Quantity { get; set; }
}
