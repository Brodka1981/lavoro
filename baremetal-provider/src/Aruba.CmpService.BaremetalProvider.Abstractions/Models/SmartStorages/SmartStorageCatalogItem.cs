using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SmartStorageCatalogItem
{
    public bool IsSoldOut { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Size { get; set; }
    public string? Snapshot { get; set; }
    public bool Replica { get; set; }
    public decimal Price { get; set; }
}
