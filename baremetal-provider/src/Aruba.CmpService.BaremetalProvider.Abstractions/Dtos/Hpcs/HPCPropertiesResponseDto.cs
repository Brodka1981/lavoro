using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Hpcs;

// FIXME: @matteo.filippa 
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class HPCPropertiesResponseDto : PropertiesBaseResponseDto
{
    public string? Admin { get; set; }
    public string? Model { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public bool RenewAllowed { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
    public string? IpAddress { get; set; }
    public string? ConfigurationMode { get; set; }
    public bool ShowVat { get; set; }
}
