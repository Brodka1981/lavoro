using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerMetadataResponseDto
{
    public Dictionary<string, string> FrameworkAiGuides { get; init; } = new();
}
