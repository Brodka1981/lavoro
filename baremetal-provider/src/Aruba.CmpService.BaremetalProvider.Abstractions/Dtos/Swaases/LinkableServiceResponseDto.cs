using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class LinkableServiceResponseDto
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Typology { get; set; }
}
