using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Responses;

public class InternalGetRegionsUseCaseResponse : MessageBusResponse<IEnumerable<Region>>
{
}
