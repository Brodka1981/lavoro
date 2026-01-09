using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SmartStorageStatistics
{
    public IEnumerable<StatisticsSmartFolder> SmartFolders { get; set; } = new List<StatisticsSmartFolder>();
    public IEnumerable<StatisticsSnapshot> Snapshots { get; set; } = new List<StatisticsSnapshot>();

    public int TotalSmartFolders { get; set; }
    public StatisticsSize? TotalSmartFoldersSize { get; set; }

    public int TotalSnapshots { get; set; }
    public StatisticsSize? TotalSnapshotsSize { get; set; }

    public StatisticsSize? TotalDiskSpace { get; set; }

    public StatisticsSize? AvailableDiskSpace { get; set; }

    public StatisticsSize? ReservedDiskSpace { get; set; }

    public StatisticsSize? TotalUsedSpace { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class StatisticsSize
{
    public StatisticsSize(string? size, long rawSize)
    {
        this.Size = size;
        this.RawSize = rawSize;
    }

    public string? Size { get; set; }
    public long RawSize { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class StatisticsSmartFolder
{
    public string? Name { get; set; }
    public StatisticsSize? Size { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class StatisticsSnapshot
{
    public string? Name { get; set; }
    public string? SmartFolderName { get; set; }
    public StatisticsSize? Size { get; set; }
    public StatisticsSize? ReferencedSize { get; set; }
}