using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;

[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class ProjectMetadata
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? CreatedBy { get; set; }
    public DateTimeOffset CreationDate { get; set; }
}
