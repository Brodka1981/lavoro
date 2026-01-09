using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Security;

#pragma warning disable CA5404

/// <summary>
/// Represents a pre-configured <see cref="TokenValidationParameters"/> to skip all the validations
/// applied to an incoming JWT <c>access_token</c>.
/// </summary>
public sealed class TokenVoidValidationParameters : TokenValidationParameters
{
    public TokenVoidValidationParameters()
    {
        RequireSignedTokens = false;

        ValidateIssuer = false;
        ValidateIssuerSigningKey = false;
        SignatureValidator = (token, parameters) => new JsonWebToken(token);

        ValidateAudience = false;
        RequireAudience = false;

        ValidateLifetime = false;
        RequireExpirationTime = false;

        ClockSkew = TimeSpan.Zero;

        NameClaimType = ClaimTypes.NameIdentifier;
    }
}
