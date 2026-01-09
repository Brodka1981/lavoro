using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class TokenProvider :
    ITokenProvider
{
    private readonly ITokenRepository tokenRepository;

    public TokenProvider(ITokenRepository tokenRepository)
    {
        this.tokenRepository = tokenRepository;
    }
    public async Task<string?> GetToken(string userId)
    {
        var token = await this.GetValidToken(userId).ConfigureAwait(false);
        return token?.Value;
    }

    public async Task SetToken(string token, string userId)
    {
        var dbToken = await this.GetInvalidToken(userId).ConfigureAwait(false);
        if (dbToken == null)
        {
            //try
            //{
                await this.tokenRepository.AddToken(userId, token).ConfigureAwait(false);

            //} catch(Exception ex)
            //{
            //    var ret = ex.StackTrace;
            //}
        }
        else
        {
            await this.tokenRepository.EditToken(dbToken.Id!, token).ConfigureAwait(false);
        }
    }
    public async Task DeleteExpiredTokens()
    {
        await this.tokenRepository.DeleteTokens(DateTimeOffset.UtcNow.AddMinutes(-10)).ConfigureAwait(false);
    }

    private async Task<Token?> GetValidToken(string userId)
    {
        var tokens = await this.tokenRepository.GetTokens(userId).ConfigureAwait(false);
        return tokens.OrderByDescending(o => o.ExpiredAt).FirstOrDefault(f => f.ExpiredAt >= DateTimeOffset.UtcNow);
    }
    private async Task<Token?> GetInvalidToken(string userId)
    {
        var tokens = await this.tokenRepository.GetTokens(userId).ConfigureAwait(false);
        return tokens.FirstOrDefault(f => f.ExpiredAt <= DateTimeOffset.UtcNow);
    }
}
