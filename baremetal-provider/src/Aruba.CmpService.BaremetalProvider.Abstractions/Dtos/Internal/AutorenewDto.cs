using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class AutorenewDto
{
    public IEnumerable<LegacyResourceIdDto> Resources { get; set; }
    public UpsertAutomaticRenewDto? AutoRenewData { get; set; }
}
