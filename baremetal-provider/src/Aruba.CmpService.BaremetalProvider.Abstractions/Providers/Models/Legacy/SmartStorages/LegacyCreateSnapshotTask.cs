using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyCreateSnapshotTask
{
    public int id { get; set; }
    public string? SmartFolderName { get; set; }
    public int? LifeTimeUnit { get; set; }
    public int LifeTimeValue { get; set; }
    public bool Enabled { get; set; }
    public LegacyCreateSnapshotTaskSchedule SnapshotSchedule { get; set; } = new();
}
