using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacySearchFilters
{
    public ResourceQueryDefinition Query { get; set; }
    public bool External { get; set; }
}
