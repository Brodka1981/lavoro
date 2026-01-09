using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class RenameDto
{
    public string? Name { get; set; }
}
