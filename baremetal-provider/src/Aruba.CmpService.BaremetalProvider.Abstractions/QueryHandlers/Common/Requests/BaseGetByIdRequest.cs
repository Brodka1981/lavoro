using Aruba.CmpService.BaremetalProvider.Abstractions.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
public abstract class BaseGetByIdRequest<TResource>
    where TResource : IResourceBase
{
    public string? ResourceId { get; set; }
    public string? UserId { get; set; }
    public string? ProjectId { get; set; }
}
