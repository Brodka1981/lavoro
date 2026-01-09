using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Tests.Repositories;
public class TokenRepositoryStub :
    ITokenRepository
{
    private List<Token> _tokens = new List<Token>();
    public TokenRepositoryStub()
    {
        for (var i = 0; i < 10; i++)
        {
            this._tokens.Add(new()
            {
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(-i).AddMinutes(2),
                Id = i.ToString(CultureInfo.InvariantCulture),
                UserId = i.ToString(CultureInfo.InvariantCulture),
                Value = i.ToString(CultureInfo.InvariantCulture)
            });
        }
    }
    public async Task<Token> AddToken(string userId, string token)
    {
        var ret = new Token()
        {
            ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(2),
            Id = userId,
            UserId = userId,
            Value = token
        };
        this._tokens.Add(ret);
        return await Task.FromResult(ret).ConfigureAwait(false);
    }

    public async Task DeleteTokens(DateTimeOffset expiredAt)
    {
        this._tokens = this._tokens.Where(w => w.ExpiredAt > expiredAt).ToList();
        await Task.CompletedTask.ConfigureAwait(false);
    }

    public async Task<Token> EditToken(string id, string token)
    {
        var dbToken = this._tokens.First(w => w.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        dbToken.Value = token;
        dbToken.ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(2);
        return await Task.FromResult(dbToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Token>> GetTokens(string userId)
    {
        var dbTokens = this._tokens.Where(w => w.UserId.Equals(userId, StringComparison.OrdinalIgnoreCase)).ToList();
        return await Task.FromResult(dbTokens).ConfigureAwait(false);
    }
}
