using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyMCIDetail : LegacyResourceDetail
{
    public MCIStatuses? Status { get; set; }
    public int? NumServices { get; set; }

    public List<LegacyBundleService>? BundleContent { get; set; }

    public string? ServerFarmCode { get; set; }

}
