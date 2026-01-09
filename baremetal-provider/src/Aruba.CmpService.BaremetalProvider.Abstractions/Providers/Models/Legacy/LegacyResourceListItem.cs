using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public abstract class LegacyResourceListItem
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public IEnumerable<string> IncludedInFolders { get; set; } = new List<string>();
    public DateTime? ExpirationDate { get; set; }
}
