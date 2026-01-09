using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Microsoft.AspNetCore.DataProtection;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class EncryptProvider :
    IEncryptProvider
{
    private readonly IDataProtectionProvider dataProtectionProvider;

    public EncryptProvider(IDataProtectionProvider dataProtectionProvider)
    {
        this.dataProtectionProvider = dataProtectionProvider;
    }

    public async Task<T?> Decrypt<T>([NotNull] DataProtectionPurposes purpose, string value)
    {
        var protector = this.dataProtectionProvider.CreateProtector(purpose.Value);
        var decryptedSerializedData = protector.Unprotect(value);
        if (!string.IsNullOrWhiteSpace(decryptedSerializedData))
        {
            var ret = decryptedSerializedData.Deserialize<T>();
            return await Task.FromResult(ret).ConfigureAwait(false);
        }
        return default(T);
    }

    public async Task<string> Encrypt<T>([NotNull] DataProtectionPurposes purpose, T value)
    {
        var protector = this.dataProtectionProvider.CreateProtector(purpose.Value);
        var ret = protector.Protect(value.Serialize());
        return await Task.FromResult(ret).ConfigureAwait(false);
    }
}
