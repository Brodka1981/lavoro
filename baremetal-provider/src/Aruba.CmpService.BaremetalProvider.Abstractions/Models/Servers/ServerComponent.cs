using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServerComponent
{
    public string? Name { get; set; }
    public int Quantity { get; set; }

}
