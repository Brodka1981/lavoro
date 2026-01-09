using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyBundleService
{
    public int? ServiceID { get; set; }
    public string? ServiceName { get; set; }
    public BundleServiceModuleType? ServiceType { get; set; }
    public BundleServiceStatuses? ServiceStatus { get; set; }
}
