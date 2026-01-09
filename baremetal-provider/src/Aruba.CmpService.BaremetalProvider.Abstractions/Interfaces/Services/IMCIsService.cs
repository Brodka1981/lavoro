using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
public interface IMCIsService
{
    Task<ServiceResult<MCIList>> Search(MCISearchFilterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<MCI>> GetById(BaseGetByIdRequest<MCI> request, CancellationToken cancellationToken);
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<MCICatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<MCI>> GetContentById(MCIContentByIdRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<MCI>> GetByIdWithPrices(MCIByIdRequest request, CancellationToken cancellationToken);
}
