
using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalMCICatalog
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
    public IEnumerable<InternalMCICatalogData> Data { get; set; } = new List<InternalMCICatalogData>();
    /// <summary>
    /// Addons
    /// </summary>
    public InternalMCICatalogFirewall Firewall { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalMCICatalogFirewall
{
    /// <summary>
    /// Addon Model
    /// </summary>
    public string? Model { get; set; }
    /// <summary>
    /// Addon data by language
    /// </summary>
    public IEnumerable<InternalMCIFirewallData> Data { get; set; } = new List<InternalMCIFirewallData>();
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalMCICatalogData
{
    public string? Language { get; set; }
    public string? Cpu { get; set; }
    public string? NodeNumber { get; set; }
    public string? Ram { get; set; }
    public string? Hdd { get; set; }
    public string? HardwareName { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class InternalMCIFirewallData
{
    public string? Language { get; set; }
    public string? FirewallName { get; set; }
}