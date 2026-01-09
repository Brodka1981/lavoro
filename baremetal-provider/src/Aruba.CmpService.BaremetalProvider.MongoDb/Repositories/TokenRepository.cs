using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
public class TokenRepository :
    ITokenRepository
{
    private readonly BaremetalProviderDbContext dbContext;
    private readonly IEncryptProvider encryptProvider;

    public TokenRepository(BaremetalProviderDbContext dbContext, IEncryptProvider encryptProvider)
    {
        this.dbContext = dbContext;
        this.encryptProvider = encryptProvider;
    }
    public async Task DeleteTokens(DateTimeOffset expiredAt)
    {
        var filter = Builders<TokenEntity>.Filter.Where(d => d.ExpiredAt <= expiredAt);
        await dbContext.Tokens.DeleteManyAsync(filter).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Token>> GetTokens(string userId)
    {
        var filter = Builders<TokenEntity>.Filter.Where(d => d.UserId == userId);
        var tokens = (await dbContext.Tokens.Find(filter).ToListAsync().ConfigureAwait(false)).OrderByDescending(o => o.ExpiredAt).ToList();
        var ret = new List<Token>();
        foreach (var token in tokens)
        {
            var item = await Map(token)!.ConfigureAwait(false);
            ret.Add(item);
        }

        return ret;
    }

    public async Task<Token> AddToken(string userId, string token)
    {
        var dbToken = new TokenEntity()
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(2),
            UserId = userId,
            Token = await this.encryptProvider.Encrypt(DataProtectionPurposes.LegacyToken, token).ConfigureAwait(false)
        };
        await dbContext.Tokens.InsertOneAsync(dbToken).ConfigureAwait(false);

        return await Map(dbToken)!.ConfigureAwait(false);
    }

    public async Task<Token> EditToken(string id, string token)
    {
        var filter = Builders<TokenEntity>.Filter.Where(d => d.Id == id);
        var dbToken = await dbContext.Tokens.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        dbToken.Token = await this.encryptProvider.Encrypt(DataProtectionPurposes.LegacyToken, token).ConfigureAwait(false);
        dbToken.ExpiredAt = DateTimeOffset.UtcNow.AddMinutes(2);
        await dbContext.Tokens.ReplaceOneAsync(filter, dbToken!).ConfigureAwait(false);

        return await Map(dbToken!)!.ConfigureAwait(false);
    }

    private async Task<Token?> Map(TokenEntity? entity)
    {
        if (entity != null)
        {
            return new Token()
            {
                ExpiredAt = entity.ExpiredAt!.Value,
                UserId = entity.UserId!,
                Id = entity.Id!,
                Value = await this.encryptProvider.Decrypt<string>(DataProtectionPurposes.LegacyToken, entity.Token!).ConfigureAwait(false)
            };
        }
        return null;
    }
}
