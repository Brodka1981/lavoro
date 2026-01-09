using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class StatusValues :
    StringEnumeration
{
    private StatusValues(string value) :
        base(value)
    {

    }
    public static StatusValues InCreation => new(nameof(InCreation));
    public static StatusValues Active => new(nameof(Active));
    public static StatusValues Activating => new(nameof(Activating));
    public static StatusValues Updating => new(nameof(Updating));
    public static StatusValues Suspended => new(nameof(Suspended));
    public static StatusValues Deleting => new(nameof(Deleting));
    public static StatusValues Deleted => new(nameof(Deleted));
    public static StatusValues Failed => new(nameof(Failed));
    public static StatusValues Disabling => new(nameof(Disabling));
    public static StatusValues Disabled => new(nameof(Disabled));
    public static StatusValues Enabling => new(nameof(Enabling));
    public static StatusValues Restarting => new(nameof(Restarting));
    public static StatusValues Upgrading => new(nameof(Upgrading));
    public static StatusValues InMaintenance => new(nameof(InMaintenance));
    public static StatusValues Unavailable => new(nameof(Unavailable));
    public static StatusValues ToBeActivated => new(nameof(ToBeActivated));
    public static StatusValues PleskLicenseRemoving => new(nameof(PleskLicenseRemoving));
    public static StatusValues PleskLicenseAdding => new(nameof(PleskLicenseAdding));

}
