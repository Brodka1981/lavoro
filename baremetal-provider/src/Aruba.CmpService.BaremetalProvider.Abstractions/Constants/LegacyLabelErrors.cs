namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class LegacyLabelErrors
{
    public string Label { get; }
    public string Message { get; }

    private LegacyLabelErrors(string errorMessage, string errorCode, [NotNull] Typologies typology)
    {
        this.Message = errorMessage;
        this.Label = string.Format(CultureInfo.InvariantCulture, "PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.{1}", typology.Value.ToUpperInvariant(), errorCode);
    }

    public static LegacyLabelErrors? Create(string? errorCode, [NotNull] Typologies typology)
    {
        var errorMessage = GetErrorDescription(errorCode);
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return null;
        }
        return new LegacyLabelErrors(errorMessage, errorCode!, typology);
    }

    private static string GetErrorDescription(string? errorCode)
    {
        switch (errorCode?.ToUpperInvariant())
        {
            case "ERR_CLOUDDCS_SETCUSTOMNAME_INVALID_LENGTH":
                return "The resource name length must be be between 4 and 50 characters";
            case "ERR_CLOUDDCS_SERVER_REBOOT_MANAGED":
                return "A managed server can't be rebooted";
            case "ERR_CLOUDDCS_SERVER_REBOOT_WEBMATRIX":
                return "A webmatrix server can't be rebooted";
            case "ERR_CLOUDDCS_SERVER_REBOOT_OFFLINE":
                return "The resource is offline";
            case "ERR_CLOUDDCS_SERVER_REBOOT_REBOOT_IN_PROGRESS":
                return "The resource is rebooting";
            case "ERR_CLOUDDCS_SERVER_REBOOT_SERVICE_EXPIRED":
                return "The resource is expired";
            case "ERR_CLOUDDCS_INVALID_AUTORENEW_INPUT_DATA":
                return "Invalid autorenew data";
            case "VLD_PAYMENT_PUTAUTOMATICRENEWAL_EXPIRE_DATE_OUT_OF_RANGE":
                return "The resource is nearing expiration";
            case "ERR_PAYMENT_PUTAUTOMATICRENEWAL_FAILED":
                return "Invalid autorenew data";
            case "ERR_CLOUDDCS_SMARTSTORAGE_SETPASSWORD_INVALID":
                return "Smart storage invalid password";
            case "ERR_CLOUDDCS_SMARTSTORAGE_ENABLESERVICE_INPROGRESS":
                return "A previous operation is in progress";
            case "ERR_REVERSE_DNS_IP_EDITING_IN_PROGRESS":
                return "Ip updating in progress";
            case "ERR_CLOUDDCS_SMARTSTORAGE_DELETESNAPSHOT_FAILED":
            case "ERR_CLOUDDCS_SMARTSTORAGE_DELETESNAPSHOTTASK_FAILED":
                return "WebDAV protocol is active";
            case "ERR_CLOUDDCS_SMARTSTORAGE_CREATESNAPSHOT_OUTOFSPACE":
            case "ERR_CLOUDDCS_SMARTSTORAGE_CREATESMARTFOLDER_OUTOFSPACE":
                return "Smart storage disk out of space";
            case "ERR_CLOUDDCS_SMARTSTORAGE_DELETESMARTFOLDER_BUSY":
                return "Smart folder blocked by snapshot";
            case "ERR_CLOUDDCS_SMARTSTORAGE_DELETESMARTFOLDER_FAILED":
                return "Smart folder blocked by WebDav";
            case "ERR_CLOUDDCS_SERVICE_INACTIVE":
                return "Inactive or not existing swaas";
            case "ERR_CLOUDDCS_SWAAS_MAXAVAILABILITYREACHED":
                return "Maximum availability limit reached";
            case "ERR_CLOUDDCS_SWAAS_FRIENDLYNAME_ALREADYUSED":
                return "Custom name already used";
            case "ERR_CLOUDDCS_SWAAS_AVAILABILITY_LT0":
                return "No availability";
            case "ERR_CLOUDDCS_INVALID_PARAMETERS":
                return "Invalid parameters";
            case "ERR_MYDC_SWAAS_RESOURCEFRIENDLYNAME_ALREADYUSED":
                return "Virtual switch link already present";
            default:
                return string.Empty;
        }
    }
}

