using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Security;

public class PolicyNames :
    StringEnumeration
{
    private PolicyNames(string value) :
        base(value)
    {

    }

    public static PolicyNames Operator = new PolicyNames("OPERATOR");
}
