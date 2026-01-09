using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public abstract class UpdateIpUseCaseRequest :
    BaseUserUseCaseRequest
{
    public long Id { get; set; }
    public IpAddressDto? IpAddress { get; set; }
}
