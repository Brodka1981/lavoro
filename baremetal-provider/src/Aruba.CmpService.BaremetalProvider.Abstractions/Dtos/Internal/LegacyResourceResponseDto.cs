using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class LegacyResourceResponseDto : BaseLegacyResourceResponseDto
{
    public string? Uri { get; set; }
    public bool ShowVat { get; set; }
}
