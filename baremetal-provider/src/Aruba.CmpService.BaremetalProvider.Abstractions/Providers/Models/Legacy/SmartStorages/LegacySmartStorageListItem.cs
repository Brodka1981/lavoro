using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySmartStorageListItem :
    LegacyResourceListItem
{
    public SmartStoragesStatuses? Status { get; set; }
    public DateTime ActivationDate { get; set; }
    public string? Model { get; set; }
}
