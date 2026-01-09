using System.Diagnostics.CodeAnalysis;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Service
{
    /// <summary>
    /// Initial order
    /// </summary>
    public bool? InitialOrder { get; set; }

    /// <summary>
    /// Order item id
    /// </summary>
    public string? OrderItemId { get; set; }

    /// <summary>
    /// Product variant id 
    /// </summary>
    public string? ProductVariantId { get; set; }

    /// <summary>
    /// Product plan id
    /// </summary>
    public string? ProductPlanId { get; set; }

    /// <summary>
    /// Service type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Service name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Service id
    /// </summary>
    public string? ServiceId { get; set; }

    /// <summary>
    /// Service group id
    /// </summary>
    public string? ServiceGroupId { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }
    public int? Quantity { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public ServiceResource? Resource { get; set; }

    /// <summary>
    /// Extra info
    /// </summary>
    public SubscriptionExtraInfo? ExtraInfo { get; set; }
}
