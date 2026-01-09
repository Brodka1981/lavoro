using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos.Swaases;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

[ExcludeFromCodeCoverage(Justification = "It's a dto without logic")]
public class VirtualSwitchAddUseCaseRequest :
    BaseUserUseCaseRequest
{
    public VirtualSwitchAddDto VirtualSwitch { get; set; } = new();
}
