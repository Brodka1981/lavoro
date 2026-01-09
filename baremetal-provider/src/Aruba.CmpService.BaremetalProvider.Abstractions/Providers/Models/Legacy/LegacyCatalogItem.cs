using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyCatalogItem
{
    public string? ProductCode { get; set; }
    public string? DisplayName { get; set; }
    public string? Category { get; set; }
    public bool IsSoldOut { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal SetupFeePrice { get; set; }    
    public IEnumerable<LegacyCatalogItemConfigProduct> BaseConfigProducts { get; set; } = new List<LegacyCatalogItemConfigProduct>();
    public IEnumerable<LegacyCatalogItemFeature> Features { get; set; } = new List<LegacyCatalogItemFeature>();
}
