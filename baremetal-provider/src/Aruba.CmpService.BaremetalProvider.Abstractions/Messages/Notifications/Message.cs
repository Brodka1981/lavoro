using System.Diagnostics.CodeAnalysis;
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Notifications;
/// <summary>
/// Dto for message
/// </summary>

[ExcludeFromCodeCoverage(Justification = "It's a model class for notification without logic")]
public class Message
{
    /// <summary>
    /// Label
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Params
    /// </summary>
    public Dictionary<string, string> Params { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Url
    /// </summary>
    public UrlData? Url { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public NotificationCategories NotificationCategory { get; set; }

    /// <summary>
    /// DeliveryStatus
    /// </summary>
    public NotificationActionStatus DeliveryStatus { get; set; } = new();

    /// <summary>
    /// ReceptionStatus
    /// </summary>
    public NotificationActionStatus ReceptionStatus { get; set; } = new();

    /// <summary>
    /// ReadStatus
    /// </summary>
    public NotificationActionStatus ReadStatus { get; set; } = new();
}
