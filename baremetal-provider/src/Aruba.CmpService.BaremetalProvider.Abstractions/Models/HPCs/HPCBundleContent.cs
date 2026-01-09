using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
//TODO: check if can be refactored with MCI class
public class HPCBundleContent
{
    public int? ServiceID { get; set; }
    public string? ServiceName { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public BundleServiceModuleType? ServiceType { get; set; }
    public BundleServiceStatuses? ServiceStatus { get; set; }
    public BundleServiceTypeCategories? ServiceTypeCategory { get; set; }
}
