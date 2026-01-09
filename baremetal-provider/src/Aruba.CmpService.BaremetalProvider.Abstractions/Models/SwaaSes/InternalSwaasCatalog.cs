using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalSwaasCatalog
{
    public string? Id { get; set; }

    public string? Code { get; set; }

    public string? ItemId { get; set; }
    
    public int LinkedDevicesCount { get; set; }

    /// <summary>
    /// Catalog data by language
    /// </summary>
    public IEnumerable<InternalSwaasCatalogData> Data { get; set; } = new List<InternalSwaasCatalogData>();
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalSwaasCatalogData
{
    public string? Language { get; set; }
    public string? Model { get; set; }
}