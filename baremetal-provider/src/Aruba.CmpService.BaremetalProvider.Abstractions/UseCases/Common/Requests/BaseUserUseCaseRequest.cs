namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
public abstract class BaseUserUseCaseRequest : BaseUseCaseRequest
{
    public string? UserId { get; set; } = string.Empty;
}
