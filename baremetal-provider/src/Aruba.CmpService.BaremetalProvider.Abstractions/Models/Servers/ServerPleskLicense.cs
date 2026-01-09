using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServerPleskLicense
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public string? ServerIp { get; set; }
    public string? ActivationCode { get; set; }
    public IEnumerable<ServerPleskLicenseAddon> Addons { get; set; } = new List<ServerPleskLicenseAddon>();
}
