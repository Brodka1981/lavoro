using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Responses;
using Aruba.MessageBus.Transactions;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;

[NonTransactional]
public class DeletePleskLicenseUseCase : ServerNoResponseBaseUseCase<DeletePleskLicenseRequest, DeletePleskLicenseResponse>
{
    public DeletePleskLicenseUseCase(IServersService serversService) : base(serversService)
    {
    }

    protected override async Task<ServiceResult> ExecuteService(DeletePleskLicenseRequest request, CancellationToken cancellationToken)
    {
        return await this.ServersService.DeletePleskLicense(request, cancellationToken).ConfigureAwait(false);
    }
}