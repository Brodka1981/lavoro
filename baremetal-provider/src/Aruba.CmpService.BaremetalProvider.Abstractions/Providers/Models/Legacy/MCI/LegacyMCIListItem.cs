using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyMCIListItem : LegacyResourceListItem
{
    public MCIStatuses? Status { get; set; }
    public int? NumServices { get; set; }
    public DateTime? ActivationDate { get; set; }
    public string? ServerFarmCode { get; set; }
}
