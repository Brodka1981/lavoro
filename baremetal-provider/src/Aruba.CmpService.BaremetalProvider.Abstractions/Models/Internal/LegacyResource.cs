using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyResource : BaseLegacyResource
{
    public string Uri { get; set; }
    public bool ShowVat { get; set; }
}
