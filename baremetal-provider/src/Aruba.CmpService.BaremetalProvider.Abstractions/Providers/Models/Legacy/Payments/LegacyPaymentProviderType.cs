using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

public class LegacyPaymentProviderType : StringEnumeration
{
    private LegacyPaymentProviderType(string value) : base(value)
    { }

    public static LegacyPaymentProviderType CloudReseller => new LegacyPaymentProviderType("CLOUD_RSCST");
    public static LegacyPaymentProviderType Cloud => new LegacyPaymentProviderType("CLOUD");
}