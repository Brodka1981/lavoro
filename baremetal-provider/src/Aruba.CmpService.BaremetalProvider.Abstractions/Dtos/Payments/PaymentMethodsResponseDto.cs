using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class PaymentMethodsResponseDto :
    List<PaymentMethodResponseDto>
{
}
