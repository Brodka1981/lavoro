using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class PaymentMethodResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public LegacyPaymentType? Type { get; set; }
    public string? Value { get; set; }
}
