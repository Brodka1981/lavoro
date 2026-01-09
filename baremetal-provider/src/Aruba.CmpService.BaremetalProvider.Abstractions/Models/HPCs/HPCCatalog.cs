
using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class HPCCatalog :
    ListResponse<HPCCatalogItem>,
    ICatalog
{
}
