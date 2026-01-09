using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Notifications;
[ExcludeFromCodeCoverage(Justification = "It's a model class for notification without logic")]
public class UrlData
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, string> Params { get; set; } = new();
}
