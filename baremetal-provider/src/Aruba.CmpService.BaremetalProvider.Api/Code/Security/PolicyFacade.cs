namespace Aruba.CmpService.BaremetalProvider.Api.Code.Security;

public static class PolicyFacade
{
    public static AuthorizationPolicy OperatorPolicy()
    {
        return new AuthorizationPolicyBuilder()
                               .RequireAuthenticatedUser()
                               .RequireRole(RoleNames.Operator.Value, RoleNames.SdoOperator.Value)
                               .Build();
    }
}
