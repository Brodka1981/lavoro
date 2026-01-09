using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]

//TODO: check if can be refactored with MCI class
public class LegacyBundleService
{
    public int? ServiceID { get; set; }
    public string? ServiceName { get; set; }
    public BundleServiceModuleType? ServiceType { get; set; }
    public BundleServiceStatuses? ServiceStatus { get; set; }
}
