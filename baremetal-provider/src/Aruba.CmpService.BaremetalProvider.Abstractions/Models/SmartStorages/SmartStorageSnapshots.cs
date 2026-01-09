using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
public class SmartStorageSnapshots
{
    public int? UsedSnapshots { get; set; }
    public int? TotalSnapshots { get; set; }
    public int? AvailableSnapshots { get; set; }
    public IEnumerable<Snapshot> Snapshots { get; set; } = new List<Snapshot>();
    public IEnumerable<SnapshotTask> SnapshotTasks { get; set; } = new List<SnapshotTask>();

}

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class Snapshot
{
    public string? SnapshotId { get; set; }
    public string? Name { get; set; }
    public string? SmartFolderName { get; set; }
    public DateTime CreationDate { get; set; }
    public string? Size { get; set; }
    public string? ReferencedSize { get; set; }
    public long RawSize { get; set; }
    public long RawReferencedSize { get; set; }
    public bool ManualSnapshot { get; set; }
}


[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SnapshotTask
{
    public int SnapshotTaskId { get; set; }
    public string? SmartFolderName { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public bool Enabled { get; set; }
    public int Iterations { get; set; }
    public string? ScheduleType { get; set; }
    public Schedule? Schedule { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class Schedule
{
    public int? Minutes { get; set; }
    public int? Hours { get; set; }
    public int? DayOfMonth { get; set; }
    public int? Month { get; set; }
    public int? DayOfWeek { get; set; }
}

