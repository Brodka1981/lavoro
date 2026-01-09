using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySmartFoldersItem
{
    public string? Name { get; set; }
    public string? SmartFolderID { get; set; }
    public string? Position { get; set; }
    public string? PositionDisplay { get; set; }
    public string? UsedSpace { get; set; }
    public long RawUsedSpace { get; set; }
    public string? AvailableSpace { get; set; }
    public long RawAvailableSpace { get; set; }
    public bool Readonly { get; set; }
    public bool IsRootFolder { get; set; }
}
