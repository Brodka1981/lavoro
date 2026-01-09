using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Switches;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class SwitchPropertiesResponseDto : PropertiesBaseResponseDto
{
    public string? Admin { get; set; }
    public string? Model { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public decimal MonthlyUnitPrice { get; set; }
    public bool ShowVat { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public bool AutoRenewAllowed { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long? RenewMonths { get; set; }
    public bool RenewAllowed { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
}
