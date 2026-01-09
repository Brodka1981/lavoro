using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SwaasCatalogItemResponseDto
{
    public bool IsSoldOut { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public int? NetworksCount { get; set; }
    public int? LinkedDevicesCount { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal SetupFeePrice { get; set; }
}
