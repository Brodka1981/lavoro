namespace Aruba.CmpService.BaremetalProvider.Api.Code.Security;

public sealed class OperatorAuthorizeAttribute :
    AuthorizeAttribute
{
    public OperatorAuthorizeAttribute() : base(PolicyNames.Operator.Value)
    {

    }


}
