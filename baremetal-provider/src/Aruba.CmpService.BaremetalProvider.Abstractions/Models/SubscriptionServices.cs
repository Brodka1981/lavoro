using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SubscriptionServices
{
    /// <summary>
    /// Service basket id
    /// </summary>
    public string? ServiceBasketId { get; set; }

    /// <summary>
    /// Subscription id
    /// </summary>
    public string? SubscriptionId { get; set; }

    /// <summary>
    /// Billing Container id
    /// </summary>
    public string? BillingContainerId { get; set; }

    /// <summary>
    /// Service Group Id 
    /// </summary>
    public string? ServiceGroupId { get; set; }

    /// <summary>
    /// Order id
    /// </summary>
    public string? OrderId { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Total price
    /// </summary>
    public decimal? TotalPrice { get; set; }

    /// <summary>
    /// Billing plan
    /// </summary>
    public BillingPlan? Plan { get; set; }

    /// <summary>
    /// Services
    /// </summary>
    public IEnumerable<Service> Services { get; set; } = new List<Service>();
}
