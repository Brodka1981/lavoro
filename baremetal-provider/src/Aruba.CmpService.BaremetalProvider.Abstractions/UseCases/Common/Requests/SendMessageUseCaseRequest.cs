using Aruba.MessageBus.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public sealed class SendMessageUseCaseRequest : Aruba.MessageBus.UseCases.Requests.Request
{
    public override string MessageBusRequestId { get; set; } = string.Empty;
    public Envelope EnvelopeToSend { get; set; }

}

