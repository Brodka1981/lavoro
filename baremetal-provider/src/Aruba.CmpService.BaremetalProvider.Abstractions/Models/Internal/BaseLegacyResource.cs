using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class BaseLegacyResource
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public string? Status { get; set; }
    public string? TypologyId { get; set; }
    public string? BillingPeriod { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public decimal? MonthlyUnitPrice { get; set; }
    public string? AutoRenewDeviceId { get; set; }
    public long? AutoRenewMonths { get; set; }
}
