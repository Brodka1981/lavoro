using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class UpsertAutomaticRenewDto
{
    public string? PaymentMethodId { get; set; }
    public byte? Months { get; set; }
}
