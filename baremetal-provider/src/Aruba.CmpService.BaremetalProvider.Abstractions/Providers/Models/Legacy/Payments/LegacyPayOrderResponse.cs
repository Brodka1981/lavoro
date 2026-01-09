using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyPayOrderResponse
{
    public Guid PaymentTransactionID { get; set; }

    public LegacyPayOrderStatus Status { get; set; }

    public string? RedirectURL { get; set; }
}