using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using Aruba.CmpService.BaremetalProvider.Tests.Repositories;
using FluentAssertions;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services;
public class TokenProviderTests : TestBase
{
    public TokenProviderTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<ITokenProvider, TokenProvider>();
        services.AddScoped<ITokenRepository, TokenRepositoryStub>();
    }

    [Fact]
    [Unit]
    public async Task Search_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var tokenProvider = provider.GetRequiredService<ITokenProvider>();

        var token = await tokenProvider.GetToken("0").ConfigureAwait(false);
        token.Should().NotBeNull();

    }

    [Fact]
    [Unit]
    public async Task Search_NotFound()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var tokenProvider = provider.GetRequiredService<ITokenProvider>();

        var token = await tokenProvider.GetToken("2").ConfigureAwait(false);
        token.Should().BeNull();
    }

    [Fact]
    [Unit]
    public async Task SetToken_NewUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var tokenProvider = provider.GetRequiredService<ITokenProvider>();

        await tokenProvider.SetToken("111", "111").ConfigureAwait(false);
        var token = await tokenProvider.GetToken("111").ConfigureAwait(false);
        token.Should().NotBeNull();
    }

    [Fact]
    [Unit]
    public async Task SetToken_ExpiredUser()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var tokenProvider = provider.GetRequiredService<ITokenProvider>();

        await tokenProvider.SetToken("2", "2").ConfigureAwait(false);
        var token = await tokenProvider.GetToken("2").ConfigureAwait(false);
        token.Should().NotBeNull();

    }

    [Fact]
    [Unit]
    public async Task Delete_ExpiredToken()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var tokenProvider = provider.GetRequiredService<ITokenProvider>();

        await tokenProvider.DeleteExpiredTokens().ConfigureAwait(false);
        var token1 = await tokenProvider.GetToken("0").ConfigureAwait(false);
        var token2 = await tokenProvider.GetToken("2").ConfigureAwait(false);
        token1.Should().NotBeNull();
        token2.Should().BeNull();

    }
}
