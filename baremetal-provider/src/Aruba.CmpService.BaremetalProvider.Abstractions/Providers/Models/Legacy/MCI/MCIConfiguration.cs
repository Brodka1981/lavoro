
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class MCIConfiguration
{
    public string? BundleConfigurationCode { get; set; }
    public string? ProductCode { get; set; }
    public int? Quantity { get; set; }
    public BundleServiceModuleType ModuleType { get; set; }
    public decimal? Price { get; set; }
}
