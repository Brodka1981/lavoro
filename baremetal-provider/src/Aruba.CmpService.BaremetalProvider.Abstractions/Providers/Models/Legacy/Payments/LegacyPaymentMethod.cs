using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyPaymentMethod
{
    [JsonPropertyName("ID")]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? TokenMasked { get; set; }
    public LegacyPaymentStatus Status { get; set; }
    public LegacyPaymentType DeviceType { get; set; }
    public decimal? WalletAmount { get; set; }
    public decimal? WalletOvedraftLimit { get; set; }

}