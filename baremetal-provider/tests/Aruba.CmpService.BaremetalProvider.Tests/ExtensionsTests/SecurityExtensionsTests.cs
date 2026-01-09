using System.Security.Claims;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.ExtensionsTests;
public class SecurityExtensionsTests :
    TestBase
{
    public SecurityExtensionsTests(ITestOutputHelper output)
        : base(output) { }




    private ClaimsPrincipal GetPrincipal(bool authenticated = true)
    {
        var identity = authenticated ? new ClaimsIdentity("test", ClaimTypes.NameIdentifier, ClaimTypes.Role) : new ClaimsIdentity();
        identity.AddClaim(new Claim("preferred_username", "Alessandro Mostarda"));
        identity.AddClaim(new Claim("company", "AsRoma"));
        identity.AddClaim(new Claim("tenant", "Serie A"));
        identity.AddClaim(new Claim("pricelist", "Sconto"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Difensore"));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "25"));
        identity.AddClaim(new Claim("test", "1"));
        identity.AddClaim(new Claim("legacy_token", "LegacyToken"));

        return new ClaimsPrincipal(identity);
    }
    private ClaimsIdentity GetIdentity(bool authenticated = true)
    {
        return GetPrincipal(authenticated).Identity as ClaimsIdentity;
    }

    [Fact]
    [Unit]
    public void IsAuthenticate_Test()
    {
        var principal = GetPrincipal();

        principal.Identity.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    [Unit]
    public void IsNotAuthenticate_Test()
    {
        var principal = GetPrincipal(false);

        principal.Identity.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    [Unit]
    public void Principal_UserIdIsValid_Test()
    {
        var principal = GetPrincipal();

        var userId = principal.GetUserId();
        userId.Should().BeEquivalentTo("Alessandro Mostarda");
    }

    [Fact]
    [Unit]
    public void UserIdIsValid_Test()
    {
        var identity = GetIdentity();

        var userId = identity.GetUserId();
        userId.Should().BeEquivalentTo("Alessandro Mostarda");
    }

    [Fact]
    [Unit]
    public void UserIdIsValid_Null_Test()
    {
        var userId = ((ClaimsIdentity)null).GetUserId();
        userId.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void Principal_UserIdIsValid_Null_Test()
    {
        var userId = ((ClaimsPrincipal)null).GetUserId();
        userId.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Tenant_Test()
    {
        var identity = GetIdentity();

        var tenant = identity.GetTenant();
        tenant.Should().BeEquivalentTo("Serie A");
        tenant.Should().NotBeEquivalentTo("Serie B");
    }


    [Fact]
    [Unit]
    public void Principal_Tenant_Test()
    {
        var principal = GetPrincipal();

        var tenant = principal.GetTenant();
        tenant.Should().BeEquivalentTo("Serie A");
        tenant.Should().NotBeEquivalentTo("Serie B");
    }


    [Fact]
    [Unit]
    public void Tenant_Null_Test()
    {
        var company = ((ClaimsIdentity)null).GetTenant();
        company.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Tenant_Company_Null_Test()
    {
        var company = ((ClaimsPrincipal)null).GetTenant();
        company.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Company_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetCompany();
        company.Should().BeEquivalentTo("AsRoma");
        company.Should().NotBeEquivalentTo("Juventus");
    }


    [Fact]
    [Unit]
    public void Company_Null_Test()
    {
        var company = ((ClaimsIdentity)null).GetCompany();
        company.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Principal_Company_Null_Test()
    {
        var company = ((ClaimsPrincipal)null).GetCompany();
        company.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Principal_Company_Test()
    {
        var principal = GetPrincipal();

        var company = principal.GetCompany();
        company.Should().BeEquivalentTo("AsRoma");
        company.Should().NotBeEquivalentTo("Juventus");
    }

    [Fact]
    [Unit]
    public void UserName_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetUserName();
        company.Should().BeEquivalentTo("Alessandro Mostarda");
        company.Should().NotBeEquivalentTo("Antonio Verdi");
    }

    [Fact]
    [Unit]
    public void UserName_Null_Test()
    {
        var userName = ((ClaimsIdentity)null).GetUserName();
        userName.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Pricelist_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetPricelist();
        company.Should().BeEquivalentTo("Sconto");
        company.Should().NotBeEquivalentTo("Prezzo pieno");
    }

    [Fact]
    [Unit]
    public void Pricelist_Null_Test()
    {
        var pricelist = ((ClaimsIdentity)null).GetPricelist();
        pricelist.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void Role_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetRole();
        company.Should().BeEquivalentTo("Difensore");
        company.Should().NotBeEquivalentTo("Attaccante");
    }

    [Fact]
    [Unit]
    public void Role_Null_Test()
    {
        var role = ((ClaimsIdentity)null).GetRole();
        role.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Unit]
    public void Account_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetAccount();
        company.Should().BeEquivalentTo("Alessandro Mostarda");
        company.Should().NotBeEquivalentTo("Antonio Verdi");
    }

    [Fact]
    [Unit]
    public void Account_Null_Test()
    {
        var account = ((ClaimsIdentity)null).GetAccount();
        account.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Claim_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetClaim("test");
        company.Should().BeEquivalentTo("1");
        company.Should().NotBeEquivalentTo("2");
    }


    [Fact]
    [Unit]
    public void Claim_NotFound_Test()
    {
        var identity = GetIdentity();

        var company = identity.GetClaim("test2");
        company.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Claim_Null_Test()
    {
        var claim = ((ClaimsIdentity)null).GetClaim("test");
        claim.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Claim_Null_Value_Test()
    {
        var claim = ((ClaimsIdentity)null).GetClaim("test2");
        claim.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void LegacyToken_Test()
    {
        var identity = GetIdentity();

        var token = identity.GetLegacyToken();
        token.Should().BeEquivalentTo("LegacyToken");
        token.Should().NotBeEquivalentTo("NoToken");
    }


    [Fact]
    [Unit]
    public void LegacyToken_Null_Test()
    {
        var token = ((ClaimsIdentity)null).GetLegacyToken();
        token.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Principal_LegacyToken_Null_Test()
    {
        var token = ((ClaimsPrincipal)null).GetLegacyToken();
        token.Should().BeNullOrWhiteSpace();
    }


    [Fact]
    [Unit]
    public void Principal_LegacyToken_Test()
    {
        var principal = GetPrincipal();

        var token = principal.GetLegacyToken();
        token.Should().BeEquivalentTo("LegacyToken");
        token.Should().NotBeEquivalentTo("NoToken");
    }
}
