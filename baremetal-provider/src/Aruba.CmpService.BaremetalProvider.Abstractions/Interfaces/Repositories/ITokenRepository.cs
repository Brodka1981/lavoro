using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
public interface ITokenRepository
{
    Task<IEnumerable<Token>> GetTokens(string userId);
    Task<Token> AddToken(string userId, string token);
    Task<Token> EditToken(string id, string token);
    Task DeleteTokens(DateTimeOffset expiredAt);
}
