using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerPleskLicenseResponseDto
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public string? ServerIp { get; set; }
    public string? ActivationCode { get; set; }
    public IEnumerable<ServerPleskLicenseResponseAddonDto> Addons { get; set; } = new List<ServerPleskLicenseResponseAddonDto>();
}
