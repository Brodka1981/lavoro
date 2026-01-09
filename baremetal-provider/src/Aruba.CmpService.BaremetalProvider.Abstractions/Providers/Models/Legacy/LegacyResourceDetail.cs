using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public abstract class LegacyResourceDetail :
    LegacyResourceListItem
{
    public decimal MonthlyUnitPrice { get; set; }
    public DateTime? ActivationDate { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public bool AutoRenewAllowed { get; set; }
    public bool RenewAllowed { get; set; }
    public LegacyAutoRenewInfo? AutoRenewInfo { get; set; }
    public IEnumerable<LegacyComponent> Components { get; set; } = new List<LegacyComponent>();
}
