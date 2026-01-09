using System;
using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class BundleContent
{
    public int? ServiceID { get; set; }
    public string? ServiceName { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public BundleServiceModuleType? ServiceType { get; set; }
    public BundleServiceStatuses? ServiceStatus { get; set; }
    public BundleServiceTypeCategories? ServiceTypeCategory { get; set; }
}
