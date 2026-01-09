using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SmartStorageCreateSnapshotTaskDto
{
    public string? FolderName { get; set; }
    public SnapshotLifeTimeUnitTypes? LifeTimeUnitType { get; set; }
    public int? Quantity { get; set; }
    public bool Enabled { get; set; }
    public byte? Minute { get; set; }
    public byte? Hour { get; set; }
    public byte? DaysOfMonth { get; set; }
    public byte? Month { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
}

