using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalServerCatalog
{
    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Server name
    /// </summary>
    public string? ServerName { get; set; }
    
    /// <summary>
    /// Server location
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Product Code
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// Catalog data by language
    /// </summary>
    public IEnumerable<InternalServerCatalogData> Data { get; set; } = new List<InternalServerCatalogData>();
}


public class InternalServerCatalogData
{
    public string? Language { get; set; }
    public string? Cpu { get; set; }
    public string? Gpu { get; set; }
    public string? Ram { get; set; }
    public string? Hdd { get; set; }
    public string? Connectivity { get; set; }
}