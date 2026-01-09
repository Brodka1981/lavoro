using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyStatistics
{
    public IEnumerable<LegacyDataSet> SmartFolders { get; set; } = new List<LegacyDataSet>();
    public IEnumerable<LegacySnapshot> Snapshots { get; set; } = new List<LegacySnapshot>();
    public int TotalSmartFolders { get; set; }
    public long RawTotalSmartFoldersSize { get; set; }
    public string? TotalSmartFoldersSize { get; set; }
    public int TotalSnapshots { get; set; }
    public long RawTotalSnapshotsSize { get; set; }
    public string? TotalSnapshotsSize { get; set; }
    public long RawTotalDiskSpace { get; set; }
    public string? TotalDiskSpace { get; set; }
    public long RawAvailableDiskSpace { get; set; }
    public string? AvailableDiskSpace { get; set; }
    public long RawReservedDiskSpace { get; set; }
    public string? ReservedDiskSpace { get; set; }
    public string? TotalUsedSpace { get; set; }
    public long RawTotalUsedSpace { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyDataSet
{
    public string? Name { get; set; }
    public string? Size { get; set; }
    public long RawSize { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySnapshot
{
    public string? Name { get; set; }
    public string? Size { get; set; }
    public string? SmartFolderName { get; set; }
    public long RawSize { get; set; }
    public string? ReferencedSize { get; set; }
    public long RawReferencedSize { get; set; }
}