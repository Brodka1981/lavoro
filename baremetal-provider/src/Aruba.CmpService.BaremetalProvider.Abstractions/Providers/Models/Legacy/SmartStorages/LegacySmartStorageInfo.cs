using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySmartStorageInfo
{
    public IEnumerable<int>? SizeList { get; set; }
    public SmartStorageInfoStatus? Status { get; set; }
    public int? Size { get; set; }
}
