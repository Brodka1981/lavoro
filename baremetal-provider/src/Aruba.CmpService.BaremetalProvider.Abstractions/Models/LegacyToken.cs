using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class LegacyToken
{
    public string? Username { get; set; }
    public string? Token { get; set; }
    public string? TokenType { get; set; }
}
