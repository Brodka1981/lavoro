using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class PaymentMethod
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public LegacyPaymentType? Type { get; set; }
    public string? Value { get; set; }
}
