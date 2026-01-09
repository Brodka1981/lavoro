using Aruba.CmpService.BaremetalProvider.Abstractions.Dtos;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

public abstract class SetAutomaticRenewUseCaseRequest :
    BaseUserUseCaseRequest
{
    public bool Activate { get; set; }
    public AutorenewFolderAction? ActionOnFolder { get; set; }
    public string? PaymentMethodId { get; set; }
    public byte? Months { get; set; }
}
