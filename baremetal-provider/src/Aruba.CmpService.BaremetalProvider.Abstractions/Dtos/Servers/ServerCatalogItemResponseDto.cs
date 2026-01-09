using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerCatalogItemResponseDto
{
    public bool IsSoldOut { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Processor { get; set; }
    public string? Ram { get; set; }
    public string? Hdd { get; set; }
    public string? Connectivity { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal SetupFeePrice { get; set; }
    public string? Location { get; set; }
    public string? ServerName { get; set; }
}
