using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;

// FIXME: @matteo.filippa use actual properties
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class HPCContentResponseDto
{
    public int? HPCServiceID { get; set; }
    public string? HPCServiceName { get; set; }
    public BundleServiceModuleType? HPCServiceType { get; set; }
    public BundleServiceStatuses? HPCServiceStatus { get; set; }
    public BundleServiceTypeCategories? HPCServiceTypeCategory { get; set; }
}
