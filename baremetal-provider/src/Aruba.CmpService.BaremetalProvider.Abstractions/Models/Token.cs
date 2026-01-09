using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Token
{
    /// <summary>
    /// Id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// User id
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Expired at
    /// </summary>
    public DateTimeOffset ExpiredAt { get; set; }
}
