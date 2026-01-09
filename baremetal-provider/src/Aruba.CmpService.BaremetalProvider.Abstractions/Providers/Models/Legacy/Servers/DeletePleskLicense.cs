using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class DeletePleskLicense
{
    public long Id { get; set; }
    public string? LicenseCode { get; set; }
}
