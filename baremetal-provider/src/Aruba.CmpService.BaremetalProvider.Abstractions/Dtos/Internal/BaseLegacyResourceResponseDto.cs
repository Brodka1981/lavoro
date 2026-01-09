using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class BaseLegacyResourceResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? Status { get; set; }
    public string? TypologyId { get; set; }
    public string? BillingPeriod { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public long? AutoRenewMonths { get; set; }
    public string? AutoRenewDeviceId { get; set; }
}
