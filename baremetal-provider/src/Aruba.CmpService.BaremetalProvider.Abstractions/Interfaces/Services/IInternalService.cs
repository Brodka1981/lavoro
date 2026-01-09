using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Internal.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;

public interface IInternalService
{
    Task<ServiceResult<IEnumerable<LegacyResource>>> GetAllResources(InternalGetResourcesUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult> UpsertAutomaticRenew(InternalAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<AutorechargeResponse>> GetAutorecharge(InternalAutorechargeUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<IEnumerable<BaseLegacyResource>>> AdminGetAllResources(InternalAdminGetResourcesUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<IEnumerable<Region>>> GetRegions(InternalGetRegionsUseCaseRequest request, CancellationToken cancellationToken);
}
