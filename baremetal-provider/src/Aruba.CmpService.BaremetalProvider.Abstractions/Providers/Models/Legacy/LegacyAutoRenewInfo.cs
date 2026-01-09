using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyAutoRenewInfo
{
    public long DeviceId { get; set; }
    public LegacyPaymentType DeviceType { get; set; }
    public long RenewMonths { get; set; }
    
}
