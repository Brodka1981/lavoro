using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
public static class PaymentMapping
{
    #region MapToResponse

    public static PaymentMethodsResponseDto? MapToResponse(this IEnumerable<PaymentMethod> paymentMethods)
    {
        var ret = new PaymentMethodsResponseDto();
        if (paymentMethods?.Any() ?? false)
        {
            ret.AddRange(paymentMethods.Select(p => p.MapToResponse()!).Where(w => w != null).ToList());
        }
        return ret;
    }

    public static PaymentMethodResponseDto? MapToResponse(this PaymentMethod paymentMethod)
    {
        if (paymentMethod == null)
        {
            return null;
        }
        var ret = new PaymentMethodResponseDto();
        ret.Id = paymentMethod.Id;
        ret.Name = paymentMethod.Name;
        ret.Type = paymentMethod.Type;
        ret.Value = paymentMethod.Type == LegacyPaymentType.Wallet ? "Wallet" : paymentMethod.Value;
        return ret;
    }
    #endregion

    #region ToModel

    /// <summary>
    /// Map legacy payment method model to baremental provider model
    /// </summary>
    public static PaymentMethod MapToListItem(this LegacyPaymentMethod legacyPaymentMethod)
        => new PaymentMethod
        {
            Id = legacyPaymentMethod.CreateCmpDeviceId(),
            Name = legacyPaymentMethod.Name,
            Value = legacyPaymentMethod.TokenMasked,
            Type = legacyPaymentMethod.DeviceType
        };
    #endregion
}
