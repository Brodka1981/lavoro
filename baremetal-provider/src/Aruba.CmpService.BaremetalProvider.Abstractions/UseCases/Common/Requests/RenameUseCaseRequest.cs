using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public abstract class RenameUseCaseRequest :
    BaseUserUseCaseRequest
{
    public RenameDto? RenameData { get; set; }
}
