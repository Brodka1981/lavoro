using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public class UpsertAutomaticRenewUseCaseRequest :
    BaseUserUseCaseRequest
{
    public UpsertAutomaticRenewDto? RenewData { get; set; }
}
