using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServerProcessor
{
    public string? Model { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }
}
