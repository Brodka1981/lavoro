using Aruba.MessageBus.UseCases.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public abstract class BaseUseCaseRequest : Request
{
    public override string MessageBusRequestId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
}
