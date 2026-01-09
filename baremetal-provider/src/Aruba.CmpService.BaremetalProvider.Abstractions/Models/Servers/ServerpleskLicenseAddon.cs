using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServerPleskLicenseAddon
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
}
