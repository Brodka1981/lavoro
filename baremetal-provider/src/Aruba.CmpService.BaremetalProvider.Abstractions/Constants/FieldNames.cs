using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class FieldNames : StringEnumeration
{
    private FieldNames(string fieldName) : base(fieldName)
    {
    }
    public static readonly FieldNames Name = new FieldNames("Name");
    public static readonly FieldNames Restart = new FieldNames("Restart");
    public static readonly FieldNames AutomaticRenew = new FieldNames("AutomaticRenew");
    public static readonly FieldNames PaymentId = new FieldNames("PaymentId");
    public static readonly FieldNames IpAddress = new FieldNames("IpAddress");
    public static readonly FieldNames Months = new FieldNames("Months");
    public static readonly FieldNames PleskLicense = new FieldNames("PleskLicense");
    public static readonly FieldNames ServiceType = new FieldNames("ServiceType");
    public static readonly FieldNames SmartFolder = new FieldNames("SmartFolder");
    public static readonly FieldNames SmartStorageActivate = new FieldNames("SmartStorageActivate");
    public static readonly FieldNames SmartStorageChangePassword = new FieldNames("SmartStorageChangePassword");
    public static readonly FieldNames SnapshotTask = new FieldNames("SnapshotTask");
    public static readonly FieldNames Snapshot = new FieldNames("Snapshot");
    public static readonly FieldNames ServiceId = new FieldNames("ServiceId");
    public static readonly FieldNames VirtualSwitch = new FieldNames("VirtualSwitch");
    public static readonly FieldNames Location = new FieldNames("Location");
    public static readonly FieldNames VirtualSwitchLink = new FieldNames("VirtualSwitchLink");
}
