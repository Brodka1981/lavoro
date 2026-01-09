using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;

public class LegacyAutorechargeData
{
    public bool Enabled { get; set; }
    public int? DeviceId { get; set; }
    public LegacyPaymentType? DeviceType { get; set; }
    public decimal? CreditToAutoRecharge { get; set; }
}
