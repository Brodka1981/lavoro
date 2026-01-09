using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class FirewallCatalog :
    ListResponse<FirewallCatalogItem>,
    ICatalog
{
}
