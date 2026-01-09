using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class HPCCatalogItemResponseDto
{
    public bool IsSoldOut { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Processor { get; set; }
    public string? Ram { get; set; }
    public string? Hdd { get; set; }
    public string? NodeNumber { get; set; }
    public decimal Price { get; set; }
    public decimal DiscountedPrice { get; set; }
    public string? ServerName { get; set; }
    public string? FirewallCode { get; set; }
    public string? FirewallName { get; set; }
    public string? BundleConfigurationCode { get; set; }
}
