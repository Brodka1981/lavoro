using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class MCI : ResourceBase<MCIProperties>
{
    public List<BundleContent> Services { get; set; } = new List<BundleContent>();
    public int? NumServices { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
}
