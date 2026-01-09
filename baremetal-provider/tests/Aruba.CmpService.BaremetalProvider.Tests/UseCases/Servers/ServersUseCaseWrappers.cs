using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.Servers;

public class DeletePleskLicenseUseCaseWrapper :
    DeletePleskLicenseUseCase
{
    public DeletePleskLicenseUseCaseWrapper(IServersService serversService) :
        base(serversService)
    {

    }
    public async Task<ServiceResult> Execute(DeletePleskLicenseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class ServerRenameUseCaseWrapper :
    ServerRenameUseCase
{
    public ServerRenameUseCaseWrapper(IServersService serversService) :
        base(serversService)
    {

    }
    public async Task<ServiceResult> Execute(ServerRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class ServerRestartUseCaseWrapper :
    ServerRestartUseCase
{
    public ServerRestartUseCaseWrapper(IServersService serversService) :
        base(serversService)
    {

    }
    public async Task<ServiceResult> Execute(ServerRestartUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}


public class ServerUpdateIpUseCaseWrapper :
    ServerUpdateIpUseCase
{
    public ServerUpdateIpUseCaseWrapper(IServersService serversService) :
        base(serversService)
    {

    }
    public async Task<ServiceResult> Execute(ServerUpdateIpUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}