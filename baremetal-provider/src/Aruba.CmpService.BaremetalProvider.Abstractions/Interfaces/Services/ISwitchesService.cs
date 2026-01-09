using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;

public interface ISwitchesService
{
    /// <summary>
    /// Search switches
    /// </summary>
    Task<ServiceResult<SwitchList>> Search(SwitchSearchFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get switch by id
    /// </summary>
    Task<ServiceResult<Switch>> GetById(BaseGetByIdRequest<Switch> request, CancellationToken cancellationToken);

    /// <summary>
    /// Rename switch
    /// </summary>
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search switch catalog
    /// </summary>
    Task<ServiceResult<SwitchCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Set automatic renew
    /// </summary>
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
}
