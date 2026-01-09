using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Notifications;
/// <summary>
/// NotificationActionStatus
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a model class for notification without logic")]
public class NotificationActionStatus
{
    /// <summary>
    /// Completed
    /// </summary>
    public bool Completed { get; set; }

    /// <summary>
    /// Date
    /// </summary>
    public DateTime? Date { get; set; }
}
