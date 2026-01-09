using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.ResourceProvider.Common.Messages.v1.Enums;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Notifications;

[ExcludeFromCodeCoverage(Justification = "Notification will be used in the future ")]
public class Notification
{
    private const string label = "SHELL_FILE.SHARED.NOTIFICATION_MESSAGES.{resource}.{action}.{type}.{reason}";
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Message Message { get; set; } = new();

    /// <summary>
    /// Constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="deployment"></param>
    /// <param name="messageAction"></param>
    /// <param name="messageType"></param>
    /// <param name="failureReason"></param>
    /// <param name="disableReason"></param>
    /// <param name="additionalParams"></param>
    /// <param name="createUrl"></param>
    /// <returns></returns>
    public static Notification Create<T>(
        ResourceBase<T> deployment,
        NotificationMessageActions messageAction,
        NotificationMessageTypes messageType,
        Models.FailureReason? failureReason = null,
        DisableReason? disableReason = null,
        Dictionary<string, string>? additionalParams = null,
        bool createUrl = false)
    {
        deployment.ThrowIfNull();
        var ret = new Notification();
        ret.Id = deployment.Id ?? string.Empty;
        ret.UserId = deployment.CreatedBy ?? string.Empty;
        var notifMessageResource = deployment.Category?.Typology?.Name?.ToUpperInvariant() ?? string.Empty;

        ret.Message = new Message()
        {
            Label = label
                .Replace("{resource}", notifMessageResource.ToString().ToUpperInvariant(), StringComparison.InvariantCulture)
                .Replace("{action}", messageAction.ToString().ToUpperInvariant(), StringComparison.InvariantCulture)
                .Replace("{type}", messageType.ToString().ToUpperInvariant(), StringComparison.InvariantCulture),
            Params =
            {
                { "deploymentId", deployment.Id ?? string.Empty }, //id, deploymentId
                { "deploymentName", deployment.Name ?? string.Empty }, //deployment name
                { "deploymentState", deployment.Status?.State ?? string.Empty }
            },
            Type = messageType.ToString(),
            NotificationCategory = NotificationCategories.ServiceNotifications,
            DeliveryStatus =
            {
                Completed = true,
                Date = DateTime.Now,
            }
        };

        if (createUrl)
        {
            ret.Message.Url = (messageAction == NotificationMessageActions.Delete && messageType == NotificationMessageTypes.Success) ? null : BuildUrlData(deployment);
        }

        ret.Message.Params.AddRange(additionalParams);
        string stringToReplace = "{reason}";
        string stringToInsert = string.Empty;

        if (!failureReason.HasValue || messageType != NotificationMessageTypes.Danger)
        {
            stringToReplace = (disableReason.HasValue ? "" : ".") + stringToReplace;
            stringToInsert = disableReason.HasValue ? disableReason.Value.ToString().ToUpperInvariant() : string.Empty;
        }
        else
        {
            stringToInsert = failureReason.Value.ToString().ToUpperInvariant();
        }
        ret.Message.Label = ret.Message.Label.Replace(stringToReplace, stringToInsert, StringComparison.OrdinalIgnoreCase);

        //Pulizia punto finale
        while (ret.Message.Label?.EndsWith('.') ?? false)
        {
            ret.Message.Label = ret.Message.Label.Substring(0, ret.Message.Label.Length - 1);
        }

        return ret;
    }
    private static UrlData BuildUrlData<T>(ResourceBase<T> deployment)
    {
        var id = deployment.Id ?? String.Empty;
        var typology = deployment.Category?.Typology?.Id ?? string.Empty;
        var categoryName = deployment.Category?.Name ?? string.Empty;

        var urlParams = new Dictionary<string, string>()
        {
            { "id", id },
            { "typologyId",typology},
            { "categoryId",   categoryName}
        };

        var urlData = new UrlData()
        {
            Type = "Deployment",
            Params = urlParams
        };

        return urlData;
    }

}
