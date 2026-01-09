using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;

// FIXME: @matteo.filippa 
namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;

public interface IHPCsService
{
    Task<ServiceResult<HPC>> Create(HPCCreateUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<HPCList>> Search(HPCSearchFilterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<HPC>> GetById(BaseGetByIdRequest<HPC> request, CancellationToken cancellationToken);
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<HPCCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<HPC>> GetContentById(HPCContentByIdRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<HPC>> GetByIdWithPrices(HPCByIdRequest request, CancellationToken cancellationToken);
}
