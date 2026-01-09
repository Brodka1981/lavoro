using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyCreateSnapshotTaskSchedule
{
    public byte Minutes { get; set; }
    public byte? Hours { get; set; }
    public byte? DayOfMonth { get; set; }
    public byte? Month { get; set; }
    public byte? DayOfWeek { get; set; }
}
