
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalHPCCatalog
{

    /// <summary>
    /// ConfigurationCode
    /// </summary>
    public string? BundleConfigurationCode { get; set; }
    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Category
    /// </summary>
    public string? Category { get; set; }
    /// <summary>
    /// Server name
    /// </summary>
    public string? ServerName { get; set; }
    /// <summary>
    /// Server Price
    /// </summary>
    public decimal? Price { get; set; }
    /// <summary>
    /// Catalog data by language
    /// </summary>
    public IEnumerable<InternalHPCCatalogData> Data { get; set; } = new List<InternalHPCCatalogData>();
    /// <summary>
    /// Addons
    /// </summary>
    public InternalHPCCatalogFirewall Firewall { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalHPCCatalogFirewall
{
    /// <summary>
    /// Addon Model
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Addon data by language
    /// </summary>
    public IEnumerable<InternalHPCFirewallData> Data { get; set; } = new List<InternalHPCFirewallData>();
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalHPCCatalogData
{
    public string? Language { get; set; }
    public string? Cpu { get; set; }
    public string? NodeNumber { get; set; }
    public string? Ram { get; set; }
    public string? Hdd { get; set; }
    public string? HardwareName { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalHPCFirewallData
{
    public string? Language { get; set; }
    public string? FirewallName { get; set; }
}