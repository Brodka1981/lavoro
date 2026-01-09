using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class PaymentMetadataResponseDto
{
    public string? PurchaseUrl { get; set; }
    public string? RenewUrl { get; set; }
    public string? RenewAndUpgradeUrl { get; set; }
    public string? RenewAndUpgradeSmartStorageAndSwaasUrl { get; set; }
    public string? Provider { get; set; }
    public int? SddTimeLimit { get; set; }
}
