using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SmartStorageCatalog :
    ListResponse<SmartStorageCatalogItem>,
    ICatalog
{
}
