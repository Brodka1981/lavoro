using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;

[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class ProjectProperties
{
    public string? Description { get; set; }

    public bool Default { get; set; }
}
