using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LinkableService
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Typology { get; set; }
}
