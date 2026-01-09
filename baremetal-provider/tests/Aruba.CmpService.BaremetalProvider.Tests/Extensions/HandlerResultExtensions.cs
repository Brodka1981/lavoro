using Aruba.MessageBus.MessageHandlers;
using Aruba.MessageBus.Models;
using Aruba.MessageBus.UseCases;

namespace Aruba.CmpService.BaremetalProvider.Tests.Extensions;
internal static class HandlerResultExtensions
{

    internal static IReadOnlyCollection<Envelope> GetOutboxEnvelopes(this HandlerResult<MessageResult> response)
    {
        return (response?.GetType()?.GetProperty("Outbox")?.GetValue(response) as IReadOnlyCollection<Envelope>) ?? new List<Envelope>();
    }

    internal static TReturnType? GetOutboxMessageType<TReturnType>(this HandlerResult<MessageResult> response) where TReturnType : class
    {
        var outbox = (response?.GetType()?.GetProperty("Outbox")?.GetValue(response) as IReadOnlyCollection<Envelope>) ?? new List<Envelope>();

        var ret = outbox.FirstOrDefault(f =>
        {
            return typeof(TReturnType).IsAssignableFrom(f.Body.GetType());
        })?.Body as TReturnType;

        return ret;
    }

    internal static TReturnType? GetOutboxMessageType<TReturnType, TMessageType>(this HandlerResult<TMessageType> response) where TMessageType : class where TReturnType : class
    {
        var outbox = (response?.GetType()?.GetProperty("Outbox")?.GetValue(response) as IReadOnlyCollection<Envelope>) ?? new List<Envelope>();

        var ret = outbox.FirstOrDefault(f =>
        {
            return typeof(TReturnType).IsAssignableFrom(f.Body.GetType());
        })?.Body as TReturnType;

        return ret;
    }

    internal static TResponse? GetResponseMessageType<TResponse>(this HandlerResult<TResponse> request) where TResponse : class
    {
        var ret = request?.GetType()?.GetProperty("Response")?.GetValue(request) as TResponse;
        return ret;
    }
}
