using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
public static class PaymentExtensions
{
    public static string CreateCmpDeviceId(this LegacyAutoRenewInfo renewInfo)
    {
        renewInfo.ThrowIfNull();
        return CreateCmpDeviceId(renewInfo.DeviceId, renewInfo.DeviceType);
    }
    public static string CreateCmpDeviceId(this LegacyPaymentMethod paymentMethod)
    {
        paymentMethod.ThrowIfNull();
        return CreateCmpDeviceId(paymentMethod.Id, paymentMethod.DeviceType);
    }
    public static string CreateCmpDeviceId(long deviceId, LegacyPaymentType type)
    {
        return $"{deviceId}{((int)type).ToString("000", CultureInfo.InvariantCulture)}";
    }

    public static LegacyAutoRenewInfo? CreateInfo(this string? id)
    {
        return new LegacyAutoRenewInfo()
        {
            DeviceId = Convert.ToInt32(id.Substring(0, id.Length - 3), CultureInfo.InvariantCulture),
            DeviceType = Enum.Parse<LegacyPaymentType>(id.Substring(id.Length - 3))
        };
    }
}
