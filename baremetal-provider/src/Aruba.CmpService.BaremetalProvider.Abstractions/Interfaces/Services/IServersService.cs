using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using ServerCatalog = Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers.ServerCatalog;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
public interface IServersService
{
    #region user
     
    /// <summary>
    /// Search servers
    /// </summary>
    Task<ServiceResult<ServerList>> Search(ServerSearchFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Get server by id
    /// </summary>
    Task<ServiceResult<Server>> GetById(BaseGetByIdRequest<Server> request, CancellationToken cancellationToken);

    /// <summary>
    /// Rename server
    /// </summary>
    Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Delete plesk license
    /// </summary>
    Task<ServiceResult> DeletePleskLicense(DeletePleskLicenseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Restart server
    /// </summary>
    Task<ServiceResult> Restart(ServerRestartUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search ip addresses
    /// </summary>
    Task<ServiceResult<ServerIpAddressList>> SearchIpAddresses(ServerIpAddressesFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Update ip address
    /// </summary>
    Task<ServiceResult> UpdateIpAddress(UpdateIpUseCaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Search server catalog
    /// </summary>
    Task<ServiceResult<ServerCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Set automatic renew
    /// </summary>
    Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken);
    #endregion
}
