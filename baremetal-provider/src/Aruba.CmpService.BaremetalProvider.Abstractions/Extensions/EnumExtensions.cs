using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
public static class EnumExtensions
{
    [ExcludeFromCodeCoverage(Justification = "Notification will be used in the future")]
    public static FailureReason ToNotification(this ResourceProvider.Common.Messages.v1.Enums.FailureReason failureReason)
    {
        switch (failureReason)
        {
            case ResourceProvider.Common.Messages.v1.Enums.FailureReason.Operator:
                return FailureReason.KubernetesOperator;
            case ResourceProvider.Common.Messages.v1.Enums.FailureReason.Ecommerce:
                return FailureReason.EcommerceIntegration;
            case ResourceProvider.Common.Messages.v1.Enums.FailureReason.PaymentRequired:
                return FailureReason.PaymentRequired;
            default:
                throw new InvalidCastException($"Cannot convert {failureReason} to {nameof(FailureReason)}");
        }
    }
}
