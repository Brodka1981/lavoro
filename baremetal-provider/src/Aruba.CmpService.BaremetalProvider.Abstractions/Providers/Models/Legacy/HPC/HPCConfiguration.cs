
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class HPCConfiguration
{
    public string? BundleConfigurationCode { get; set; }
    public string? ProductCode { get; set; }
    public int? Quantity { get; set; }
    public BundleServiceModuleType ModuleType { get; set; }
    public decimal? Price { get; set; }
}
