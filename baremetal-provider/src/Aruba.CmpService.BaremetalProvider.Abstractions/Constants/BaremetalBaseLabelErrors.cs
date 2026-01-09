using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class BaremetalBaseLabelErrors :
    StringEnumeration
{

    protected BaremetalBaseLabelErrors(string value, [NotNull] Typologies typology) :
        base(string.Format(CultureInfo.InvariantCulture, value!, typology.Value.ToUpperInvariant()))
    {
    }

    public static BaremetalBaseLabelErrors NameMinimumLength(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.MINLENGTH", typology);
    public static BaremetalBaseLabelErrors NameMaximumLength(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.MAXLENGTH", typology);
    public static BaremetalBaseLabelErrors NameInvalidChars(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.PATTERN", typology);
    public static BaremetalBaseLabelErrors NameRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.REQUIRED", typology);
    public static BaremetalBaseLabelErrors PaymentMethodIdRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.VALIDATION_ERROR_MESSAGES.PAYMENT_METHOD.REQUIRED", typology);
    public static BaremetalBaseLabelErrors MonthsRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.VALIDATION_ERROR_MESSAGES.MONTHS.REQUIRED", typology);
    public static BaremetalBaseLabelErrors MonthsInvalid(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.VALIDATION_ERROR_MESSAGES.MONTHS.PATTERN", typology);
    public static BaremetalBaseLabelErrors FraudRisk(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.ASYNC_BE_ERROR_MESSAGES.FRAUD_RISK", typology);
    public static BaremetalBaseLabelErrors SddNotAllowed(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.ASYNC_BE_ERROR_MESSAGES.SDD_NOT_ALLOWED", typology);
    public static BaremetalBaseLabelErrors ServiceTypeRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.VALIDATION_ERROR_MESSAGES.SERVICETYPE.REQUIRED", typology);
    public static BaremetalBaseLabelErrors FolderNameMinimumLength(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_FOLDER.INPUT.ERRORS_MESSAGE.MINLENGTH", typology);
    public static BaremetalBaseLabelErrors FolderNameMaximumLength(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_FOLDER.INPUT.ERRORS_MESSAGE.MAXLENGTH", typology);
    public static BaremetalBaseLabelErrors FolderNameInvalidChars(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_FOLDER.INPUT.ERRORS_MESSAGE.NAME.NOTVALID", typology);
    public static BaremetalBaseLabelErrors FolderNameExists(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_FOLDER.INPUT.ERRORS_MESSAGE.NAME.EXISTS", typology);
    public static BaremetalBaseLabelErrors FolderNotExists(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.FOLDER.NOT.EXISTS", typology);
    public static BaremetalBaseLabelErrors FolderMaximum20(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.MAXIMUM_FOLDERS_REACHED", typology);
    public static BaremetalBaseLabelErrors FolderInsufficientDiskSpace(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERR_CLOUDDCS_SMARTSTORAGE_CREATESMARTFOLDER_OUTOFSPACE", typology);
    public static BaremetalBaseLabelErrors PasswordAndConfirmDoNotMatch(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_PASSWORD.INPUT.ERRORS_MESSAGE.PASSWORD.CONFIRMPASSWORD.DONOTMATCH", typology);
    //Labels per SnapshotsTasks
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitTypeRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitHourlyCreationTimeRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.HOURLY.CREATIONTIME.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitInvalidHourlyParameters(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.HOURLY.INVALIDPARAMETERS", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitDailyCreationTimeRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.DAILY.CREATIONTIME.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitInvalidDailyParameters(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.DAILY.INVALIDPARAMETERS", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitWeeklyCreationTimeAndDayOfWeekRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.WEEKLY.CREATIONTIME.DAYOFWEEK.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitInvalidWeeklyParameters(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.WEEKLY.INVALIDPARAMETERS", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitMonthlyCreationTimeAndDayOfMonthRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.MONTHLY.CREATIONTIME.DAYOFMONTH.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitInvalidMonthlyParameters(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.MONTHLY.INVALIDPARAMETERS", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitYearlyCreationTimeAndDayOfMonthRequired(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.YEARLY.CREATIONTIME.DAYOFMONTH.MONTH.REQUIRED", typology);
    public static BaremetalBaseLabelErrors SnapshotLifeTimeUnitInvalidYearlyParameters(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.LIFETIMEUNITTYPE.YEARLY.INVALIDPARAMETERS", typology);
    public static BaremetalBaseLabelErrors SnapshotQuantityExceedsAvailable(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.LEGACY.INPUT.ERRORS_MESSAGE.SNAPSHOT.TASK.QUANTITY.EXCEEDS.AVAILABLE", typology);

    public static BaremetalBaseLabelErrors IpUpdating(Typologies typology) => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.TABS.IP_ADDRESS.MODAL.ACTION_MESSAGES.EDIT.ERR_REVERSE_DNS_IP_EDITING_IN_PROGRESS", typology);

    //Virtua Switch
    public static BaremetalBaseLabelErrors VirtualSwitchLocationRequired() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.REQUIRED", Typologies.Swaas);
    public static BaremetalBaseLabelErrors VirtualSwitchLocationNotFound() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CHANGE_NAME.INPUT.ERRORS_MESSAGE.REQUIRED", Typologies.Swaas);

    //Virtual switch link    
    public static BaremetalBaseLabelErrors ServiceToConnectIdRequired() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_LINK.INPUT.ERRORS_MESSAGE.SERVICE_TO_CONNECT_ID_REQUIRED", Typologies.Swaas);
    public static BaremetalBaseLabelErrors ServiceToConnectTypologyRequired() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_LINK.INPUT.ERRORS_MESSAGE.SERVICE_TO_CONNECT_TYPOLOGY_REQUIRED", Typologies.Swaas);
    public static BaremetalBaseLabelErrors ServiceToConnectNotFound() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_LINK.INPUT.ERRORS_MESSAGE.SERVICE_TO_CONNECT_NOT_FOUND", Typologies.Swaas);
    public static BaremetalBaseLabelErrors ServiceToConnectInvalidRegion() => new BaremetalBaseLabelErrors("PROVISIONING_FILE.PROVISIONING.{0}.DETAIL.MODAL.CREATE_LINK.INPUT.ERRORS_MESSAGE.SERVICE_TO_CONNECT_INVALID_REGION", Typologies.Swaas);
}

