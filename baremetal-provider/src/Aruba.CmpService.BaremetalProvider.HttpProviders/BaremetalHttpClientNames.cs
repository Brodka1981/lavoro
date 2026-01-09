using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders;

public class BaremetalHttpClientNames : StringEnumeration
{
    private BaremetalHttpClientNames(string value) : base(value)
    { }

    public static BaremetalHttpClientNames ProfileProvider => new(nameof(ProfileProvider));
    public static BaremetalHttpClientNames CatalogueProvider => new(nameof(CatalogueProvider));
    public static BaremetalHttpClientNames ResourceManagerProvider => new(nameof(ResourceManagerProvider));
    public static BaremetalHttpClientNames LegacyProvider => new(nameof(LegacyProvider));
    public static BaremetalHttpClientNames LegacyAuthenticationProvider => new(nameof(LegacyAuthenticationProvider));
    public static BaremetalHttpClientNames AdminLegacyProvider => new(nameof(AdminLegacyProvider));
}
