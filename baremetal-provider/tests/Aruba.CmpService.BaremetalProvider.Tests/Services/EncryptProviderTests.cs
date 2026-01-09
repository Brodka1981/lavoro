using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;

namespace Aruba.CmpService.BaremetalProvider.Tests.Services;
public class EncryptProviderTests : TestBase
{
    public EncryptProviderTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        services.AddScoped<IEncryptProvider, EncryptProvider>();
        services.AddScoped<IDataProtectionProvider, DataProtectionProvider>();
        services.AddScoped<IDataProtector, DataProtector>();
    }

    [Fact]
    [Unit]
    public async Task Encrypt_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var encryptProvider = provider.GetRequiredService<IEncryptProvider>();

        var token = await encryptProvider.Encrypt(DataProtectionPurposes.LegacyToken, "a");
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().Be("ImEi");
    }

    [Fact]
    [Unit]
    public async Task Decrypt_Success()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var encryptProvider = provider.GetRequiredService<IEncryptProvider>();

        var token = await encryptProvider.Decrypt<string>(DataProtectionPurposes.LegacyToken, "ImEi");
        token.Should().NotBeNullOrWhiteSpace();
        token.Should().Be("a");
    }

    [Fact]
    [Unit]
    public async Task Decrypt_null()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var encryptProvider = provider.GetRequiredService<IEncryptProvider>();

        var token = await encryptProvider.Decrypt<string>(DataProtectionPurposes.LegacyToken, "");
        token.Should().BeNullOrWhiteSpace();
    }
}

internal class DataProtectionProvider :
    IDataProtectionProvider
{
    public IDataProtector CreateProtector(string purpose)
    {
        return new DataProtector().CreateProtector(purpose);
    }
}

internal class DataProtector :
    IDataProtector
{

    public IDataProtector CreateProtector(string purpose)
    {
        return new DataProtector();
    }

    public byte[] Protect(byte[] plaintext)
    {
        return plaintext;
    }

    public byte[] Unprotect(byte[] protectedData)
    {
        return protectedData;
    }
}
