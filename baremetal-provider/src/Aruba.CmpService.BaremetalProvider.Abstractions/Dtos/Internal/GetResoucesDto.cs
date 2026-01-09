using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class GetResoucesDto
{
    public IEnumerable<LegacyResourceIdDto>? Services { get; set; }
    public bool GetPrices { get; set; }
}
