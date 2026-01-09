using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerPropertiesResponseDto : PropertiesBaseResponseDto
{
    public string? Model { get; set; }
    public string? Processor { get; set; }
    public string? OperatingSystem { get; set; }
    public string? Ram { get; set; }
    public string? Gpu { get; set; }
    public string? Hdd { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public ServerPleskLicenseResponseDto? PleskLicense { get; set; }
    public decimal MonthlyUnitPrice { get; set; }
    public bool ShowVat { get; set; } 
    public bool AutoRenewEnabled { get; set; }
    public bool AutoRenewAllowed { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long RenewMonths { get; set; }
    public bool RenewAllowed { get; set; }
    public bool UpgradeAllowed { get; set; }
    public bool RenewUpgradeAllowed { get; set; }
    public string? ModelTypeCode { get; set; }
    public string? OriginalName { get; set; }
    public string? ServerName { get; set; }
    public string? BundleProjectName { get; set; }
    public string? BundleCode { get; set; }
    public IEnumerable<string> Folders { get; set; } = new List<string>();
    public IEnumerable<ServerComponentResponseDto> Components { get; set; } = new List<ServerComponentResponseDto>();
}
