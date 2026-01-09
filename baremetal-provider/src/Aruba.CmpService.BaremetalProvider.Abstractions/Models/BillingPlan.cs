using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class BillingPlan
{
    /// <summary>
    /// Billing period
    /// </summary>
    public string? BillingPeriod { get; set; }
}
