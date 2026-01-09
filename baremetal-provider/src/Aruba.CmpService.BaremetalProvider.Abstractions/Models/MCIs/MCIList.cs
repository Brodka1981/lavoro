using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class MCIList :
    ListResponse<MCI>
{
}
