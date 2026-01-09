using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyServerPleskLicense
{
    public string? LicenseCode { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? ActivationDate { get; set; }
    public string? ActivationCode { get; set; }
    public bool isAddon { get; set; }
}
