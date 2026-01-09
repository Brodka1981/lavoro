using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySnapshots
{
    public int UsedSnapshots { get; set; }
    public int TotalSnapshots { get; set; }
    public int AvailableSnapshots { get; set; }
    public IEnumerable<LegacyManualSnapshots> Snapshots { get; set; } = new List<LegacyManualSnapshots>();
    public IEnumerable<LegacySnapshotsTasks> SnapshotTasks { get; set; } = new List<LegacySnapshotsTasks>();
}

public class LegacyManualSnapshots
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
public class LegacySnapshotsTasks
{
    public int SnapshotTaskId { get; set; }
    public string? SmartFolderName { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public bool Enabled { get; set; }
    public int Iterations { get; set; }
    public string? ScheduleType { get; set; }
    public LegacySchedule? Schedule { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class LegacySchedule
{
    public int? Minutes { get; set; }
    public int? Hours { get; set; }
    public int? DayOfMonth { get; set; }
    public int? Month { get; set; }
    public int? DayOfWeek { get; set; }
}