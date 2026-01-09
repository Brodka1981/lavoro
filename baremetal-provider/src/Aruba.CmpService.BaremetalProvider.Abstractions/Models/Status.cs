using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Status
{
    /// <summary>
    /// State
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTimeOffset? CreationDate { get; set; }

    /// <summary>
    /// Disable status info
    /// </summary>
    public DisableStatusInfo? DisableStatusInfo { get; init; } = new DisableStatusInfo();

    public static Status Create(StatusValues state)
    {
        ArgumentNullException.ThrowIfNull(state);
        return new Status()
        {
            CreationDate = DateTimeOffset.UtcNow,
            State = state.Value

        };
    }
}