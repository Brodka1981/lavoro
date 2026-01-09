using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SwitchCatalogItem
{
    public bool IsSoldOut { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Ports { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal SetupFeePrice { get; set; }
    public string? Location { get; set; }
}
