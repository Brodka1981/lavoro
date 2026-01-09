using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageSnapshotsResponseDto
{
    public int? UsedSnapshots { get; set; }
    public int? TotalSnapshots { get; set; }
    public int? AvailableSnapshots { get; set; }
    public IEnumerable<SnapshotDto> Snapshots { get; set; } = new List<SnapshotDto>();
    public IEnumerable<SnapshotTasksDto> SnapshotTasks { get; set; } = new List<SnapshotTasksDto>();
}

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SnapshotDto
{
    public string? SnapshotId { get; set; }
    public string? Name { get; set; }
    public string? SmartFolderName { get; set; }
    public DateTimeOffset CreationDate { get; set; }
    public string? Size { get; set; }
    public string? ReferencedSize { get; set; }
    public long RawSize { get; set; }
    public long RawReferencedSize { get; set; }
    public bool ManualSnapshot { get; set; }
}


[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SnapshotTasksDto
{
    public int SnapshotTaskId { get; set; }
    public string? SmartFolderName { get; set; }
    public DateTimeOffset? CreationDate { get; set; }
    public bool Enabled { get; set; }
    public int Iterations { get; set; }
    public string? ScheduleType { get; set; }
    public ScheduleDto? Schedule { get; set; }
}

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ScheduleDto
{
    public int? Minutes { get; set; }
    public int? Hours { get; set; }
    public int? DayOfMonth { get; set; }
    public int? Month { get; set; }
    public int? DayOfWeek { get; set; }
}
