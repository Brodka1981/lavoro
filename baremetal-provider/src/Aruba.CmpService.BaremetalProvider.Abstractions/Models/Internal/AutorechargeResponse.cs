using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class AutorechargeResponse
{
    public bool Enabled { get; set; }
    public int? DeviceId { get; set; }
    public LegacyPaymentType? DeviceType { get; set; }
    public decimal? CreditToAutoRecharge { get; set; }
}
