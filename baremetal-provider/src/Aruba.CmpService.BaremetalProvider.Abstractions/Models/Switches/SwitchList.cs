using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SwitchList :
    ListResponse<Switch>
{
}
