using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Security;

public class RoleNames :
    StringEnumeration
{
    private RoleNames(string value) :
        base(value)
    {

    }

    public static RoleNames Operator = new RoleNames("OPERATOR");
    public static RoleNames SdoOperator = new RoleNames("SDO_OPERATOR");
}
