using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class LegacyResourceIdDto
{
    public int Id { get; set; }
    public string? TypologyId { get; set; }
}
