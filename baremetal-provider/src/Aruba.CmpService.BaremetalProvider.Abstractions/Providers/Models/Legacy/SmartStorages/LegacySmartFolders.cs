using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySmartFolders
{
    public int UsedSmartFolders { get; set; }
    public int TotalSmartFolders { get; set; }
    public int AvailableSmartFolders { get; set; }
    public IEnumerable<LegacySmartFoldersItem> SmartFolders { get; set; } = new List<LegacySmartFoldersItem>();
}
