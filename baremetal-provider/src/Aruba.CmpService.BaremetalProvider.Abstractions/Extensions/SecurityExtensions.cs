using System.Security.Claims;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;

public static class SecurityExtensions
{
    private const string userName = "preferred_username";
    private const string company = "company";
    private const string account = "preferred_username";
    private const string tenant = "tenant";
    private const string pricelist = "pricelist";
    private const string legacyToken = "legacy_token";

    private const string protocol = "http://";
    private const string role = $"{protocol}schemas.microsoft.com/ws/2008/06/identity/claims/role";


    /// <summary>
    /// Legge il Pricelist dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetPricelist(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(pricelist)!;

    /// <summary>
    /// Legge il Role dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetRole(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(role)!;

    /// <summary>
    /// Legge il tenant dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetTenant(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(tenant)!;

    /// <summary>
    /// Legge l'account dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetAccount(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(account)!;

    /// <summary>
    /// Legge lo username dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetUserName(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(userName)!;

    /// <summary>
    /// Legge il claim dal token
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetCompany(this ClaimsIdentity claimsIdentity)
        => claimsIdentity?.GetClaim(company)!;

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <param name="claimName"></param>
    /// <returns></returns>
    public static string GetClaim(this ClaimsIdentity claimsIdentity, string claimName)
        => claimsIdentity?.FindFirst(claimName)?.Value ?? string.Empty;

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public static string GetTenant(this ClaimsPrincipal? claimsPrincipal)
    {
        return (claimsPrincipal?.Identity as ClaimsIdentity)?.GetTenant()!;
    }

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public static string GetCompany(this ClaimsPrincipal? claimsPrincipal)
    {
        return (claimsPrincipal?.Identity as ClaimsIdentity)?.GetCompany()!;
    }


    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetUserId(this ClaimsIdentity? claimsIdentity)
        => claimsIdentity?.GetClaim(userName)!;

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public static string GetUserId(this ClaimsPrincipal? claimsPrincipal)
    {
        return (claimsPrincipal?.Identity as ClaimsIdentity).GetUserId()!;
    }

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public static string GetLegacyToken(this ClaimsPrincipal? claimsPrincipal)
    {
        return (claimsPrincipal?.Identity as ClaimsIdentity).GetLegacyToken()!;
    }

    /// <summary>
    /// Legge i claim
    /// </summary>
    /// <param name="claimsIdentity"></param>
    /// <returns></returns>
    public static string GetLegacyToken(this ClaimsIdentity? claimsIdentity)
        => claimsIdentity?.GetClaim(legacyToken)!;
}
