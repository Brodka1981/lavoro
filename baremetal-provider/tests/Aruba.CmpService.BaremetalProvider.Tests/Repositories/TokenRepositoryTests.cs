using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Repositories;
public class TokenRepositoryTests : TestBase
{
    public TokenRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ITokenRepository, TokenRepositoryStub>();
    }

    [Fact]
    [Unit]
    public async Task Add_Token_NewUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var repository = provider.GetRequiredService<ITokenRepository>();

        await repository.AddToken("111", "111").ConfigureAwait(false);

        var tokens = await repository.GetTokens("111").ConfigureAwait(false);
        tokens.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Add_Token_ExistingUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var repository = provider.GetRequiredService<ITokenRepository>();

        await repository.AddToken("1", "111").ConfigureAwait(false);

        var tokens = await repository.GetTokens("1").ConfigureAwait(false);
        tokens.Should().NotBeNull().And.HaveCount(2);
    }

    [Fact]
    [Unit]
    public async Task Edit_Token_ExistingUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var repository = provider.GetRequiredService<ITokenRepository>();

        await repository.EditToken("1", "111").ConfigureAwait(false);

        var tokens = await repository.GetTokens("1").ConfigureAwait(false);
        tokens.Should().NotBeNull().And.HaveCount(1);
    }

    [Fact]
    [Unit]
    public async Task Edit_Token_NonExistingUser_Failed()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var repository = provider.GetRequiredService<ITokenRepository>();
        bool edited = true;
        try
        {
            await repository.EditToken("11", "111").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            edited = false;
        }

        edited.Should().BeFalse();
    }

    [Fact]
    [Unit]
    public async Task Delete_Expired_Tokens()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();
        var repository = provider.GetRequiredService<ITokenRepository>();
        await repository.DeleteTokens(DateTimeOffset.UtcNow.AddDays(-2)).ConfigureAwait(false);

        var tokens = await repository.GetTokens("1").ConfigureAwait(false);
        tokens.Should().NotBeNull().And.HaveCount(1);

        tokens = await repository.GetTokens("5").ConfigureAwait(false);
        tokens.Should().NotBeNull().And.HaveCount(0);

    }
}
