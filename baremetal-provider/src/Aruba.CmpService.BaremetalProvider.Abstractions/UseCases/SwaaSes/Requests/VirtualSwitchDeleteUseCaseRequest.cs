using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchDeleteUseCaseRequest :
    BaseUserUseCaseRequest
{
    public string? Id { get; set; }
}
