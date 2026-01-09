using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Swaases.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SwaaSes.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
public interface ISwaasesService
{
    /// <summary>
    /// Get swaas by id
    /// </summary>
    Task<ServiceResult<Swaas>> GetById(BaseGetByIdRequest<Swaas> request, CancellationToken cancellationToken);

    /// <summary>
    /// Rename swaas
    /// </summary>
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search swaas
    /// </summary>
    Task<ServiceResult<SwaasList>> Search(SwaasSearchFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Set automatic renew
    /// </summary>
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search swaas catalog
    /// </summary>
    Task<ServiceResult<SwaasCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search virtual switch
    /// </summary>
    Task<ServiceResult<VirtualSwitchList>> GetVirtualSwitches(VirtualSwitchSearchFilterRequest request);

    /// <summary>
    /// Get virtual switch
    /// </summary>
    Task<ServiceResult<VirtualSwitch>> GetVirtualSwitch(VirtualSwitchGetByIdRequest request);

    /// <summary>
    /// Add virtual switch
    /// </summary>
    Task<ServiceResult<VirtualSwitch>> AddVirtualSwitch(VirtualSwitchAddUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// edit virtual switch
    /// </summary>
    Task<ServiceResult<VirtualSwitch>> EditVirtualSwitch(VirtualSwitchEditUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete virtual switch
    /// </summary>
    Task<ServiceResult> DeleteVirtualSwitch(VirtualSwitchDeleteUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get virtual switch links
    /// </summary>
    Task<ServiceResult<VirtualSwitchLinkList>> GetVirtualSwitchLinks(VirtualSwitchLinkSearchFilterRequest request);

    /// <summary>
    /// Get virtual switch link
    /// </summary>
    Task<ServiceResult<VirtualSwitchLink>> GetVirtualSwitchLink(VirtualSwitchLinkGetByIdRequest request);

    /// <summary>
    /// Add virtual switch link
    /// </summary>
    Task<ServiceResult<VirtualSwitchLink>> AddVirtualSwitchLink(VirtualSwitchLinkAddUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete virtual switch link
    /// </summary>
    Task<ServiceResult> DeleteVirtualSwitchLink(VirtualSwitchLinkDeleteUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get linkable services for the given virtual switch
    /// </summary>
    Task<ServiceResult<List<LinkableService>>> GetLinkableServices(VirtualSwitchGetLinkableServicesRequest request, CancellationToken cancellationToken);
}
