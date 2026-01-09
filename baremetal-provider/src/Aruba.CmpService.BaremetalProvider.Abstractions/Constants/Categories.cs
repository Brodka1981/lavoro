using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Constants;

[ExcludeFromCodeCoverage(Justification = "It's a constants class without logic")]
public class Categories : StringEnumeration
{
    private Categories(string value) : base(value)
    {

    }

    public static Categories BaremetalServer => new Categories(nameof(BaremetalServer));
    public static Categories BaremetalNetwork => new Categories(nameof(BaremetalNetwork));
    public static Categories BaremetalSmartStorage => new Categories(nameof(BaremetalSmartStorage));
    public static Categories MetalCloudInfrastructure => new Categories(nameof(MetalCloudInfrastructure));

}
