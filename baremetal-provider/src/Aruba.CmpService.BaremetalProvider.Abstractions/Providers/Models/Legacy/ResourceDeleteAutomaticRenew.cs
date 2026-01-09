using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ResourceDeleteAutomaticRenew
{
    public long Id { get; set; }
    public virtual bool EnableAutoRenew { get; }
}
