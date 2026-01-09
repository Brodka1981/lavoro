using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;

public class MessageBusResponse
{
    public MessageBusResponseTypes Type { get; set; }
    public Exception? Exception { get; set; }
    public IEnumerable<BadRequestError> Errors { get; set; } = new List<BadRequestError>();
    public object? Id { get; set; }

}

public class MessageBusResponse<T> : MessageBusResponse
{
    public T? Value { get; set; }
}
public abstract class EmptyMessageBusResponse : MessageBusResponse<object>
{

}
