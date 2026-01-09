using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Servers;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class ServerComponentResponseDto : PropertiesBaseResponseDto
{
    public string? Name { get; set; }
    public int Quantity { get; set; }
}
