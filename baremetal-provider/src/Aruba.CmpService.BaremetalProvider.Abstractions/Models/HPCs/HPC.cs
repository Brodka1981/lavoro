using System.Diagnostics.CodeAnalysis;

// FIXME: @matteo.filippa 
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class HPC : ResourceBase<HPCProperties>
{
    public List<HPCBundleContent> Services { get; set; } = new List<HPCBundleContent>();
    public int? NumServices { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
}
