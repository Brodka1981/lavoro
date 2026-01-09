using Aruba.MessageBus.MessageHandlers;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
internal static class HandlerResponse
{
    internal static HandlerResult<MessageResult> Executed() =>
        HandlerResult.Executed(new MessageResult());
}
