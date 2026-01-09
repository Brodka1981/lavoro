using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Mcis;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class MCIContentResponseDto
{
    public int? MCIServiceID { get; set; }
    public string? MCIServiceName { get; set; }
    public BundleServiceModuleType? MCIServiceType { get; set; }
    public BundleServiceStatuses? MCIServiceStatus { get; set; }
    public BundleServiceTypeCategories? MCIServiceTypeCategory { get; set; }
}
