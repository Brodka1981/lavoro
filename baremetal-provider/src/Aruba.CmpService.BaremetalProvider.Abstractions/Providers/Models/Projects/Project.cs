using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;

[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class Project
{
    public ProjectMetadata Metadata { get; set; }

    public ProjectProperties Properties { get; set; }

}
