using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
public interface IEncryptProvider
{
    Task<string> Encrypt<T>(DataProtectionPurposes purpose, T value);
    Task<T?> Decrypt<T>(DataProtectionPurposes purpose, string value);
}
