namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
public interface ITokenProvider
{
    Task SetToken(string token, string userId);
    Task<string?> GetToken(string userId);
    Task DeleteExpiredTokens();
}
