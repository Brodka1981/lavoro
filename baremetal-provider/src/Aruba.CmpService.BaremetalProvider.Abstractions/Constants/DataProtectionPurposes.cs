using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class DataProtectionPurposes : StringEnumeration
{
    private DataProtectionPurposes(string purpose) : base(purpose)
    {

    }

    public static readonly DataProtectionPurposes LegacyToken = new(nameof(LegacyToken));
}
