using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;
[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerPleskLicenseResponseAddonDto
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
}
