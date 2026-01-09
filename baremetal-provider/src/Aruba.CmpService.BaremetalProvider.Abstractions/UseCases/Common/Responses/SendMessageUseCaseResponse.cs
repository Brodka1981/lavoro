namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Responses;
//public class SendMessageUseCaseResponse :
//    EmptyMessageBusResponse
//{
//}
public abstract record class SendMessageUseCaseResponse
{
    public sealed record class Success(object SentEvent) : SendMessageUseCaseResponse;
}

