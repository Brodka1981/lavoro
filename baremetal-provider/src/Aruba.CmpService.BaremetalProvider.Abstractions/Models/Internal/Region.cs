using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Region
{
    public string? Id { get; set; }
    public string? Code { get; set; }
}
