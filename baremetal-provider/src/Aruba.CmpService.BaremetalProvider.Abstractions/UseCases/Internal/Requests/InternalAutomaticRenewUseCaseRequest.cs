using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class InternalAutomaticRenewUseCaseRequest :
    UpsertAutomaticRenewUseCaseRequest
{
    public IEnumerable<LegacyResourceIdDto> Resources { get; set; }
}
