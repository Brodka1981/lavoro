using Aruba.MessageBus.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Utils;

public static class MessageUtils
{
    public static Envelope EnvelopeCreateWithSubject(string? subject, object body)
    {
        var clonedHeaders = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [Aruba.MessageBus.CloudEvents.HeaderNames.Standard.V1.Subject] = subject,
        };
        return new Envelope(clonedHeaders, body);

    }
}
