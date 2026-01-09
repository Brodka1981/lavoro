using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchEditUseCaseRequest :
    BaseUserUseCaseRequest
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}
