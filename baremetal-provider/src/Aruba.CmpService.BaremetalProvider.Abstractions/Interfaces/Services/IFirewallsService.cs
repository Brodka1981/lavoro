using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
public interface IFirewallsService
{
    /// <summary>
    /// Get server list 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult<FirewallList>> Search(FirewallSearchFilterRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Get server by id
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult<Firewall>> GetById(BaseGetByIdRequest<Firewall> request, CancellationToken cancellationToken);
    /// <summary>
    /// rename server
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Get server ip addresses
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult<FirewallIpAddressList>> SearchIpAddresses(FirewallIpAddressesFilterRequest request, CancellationToken cancellationToken);
    Task<ServiceResult<FirewallVlanIdList>> GetVlanIds(FirewallVlanIdFilterRequest request, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update server ip address
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult> UpdateIpAddress(UpdateIpUseCaseRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Get servers catalog
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult<FirewallCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);
    /// <summary>
    /// Set automatic renew
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
}
