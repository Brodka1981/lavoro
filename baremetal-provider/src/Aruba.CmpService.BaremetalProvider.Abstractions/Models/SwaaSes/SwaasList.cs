using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SwaasList :
    ListResponse<Swaas>
{
}
