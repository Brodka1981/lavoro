using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.SmartStorages;

public class SmartStorageActivateUseCaseWrapper :
    SmartStorageActivateUseCase
{
    public SmartStorageActivateUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageActivateUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageApplySnapshotUseCaseWrapper :
    SmartStorageApplySnapshotUseCase
{
    public SmartStorageApplySnapshotUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageApplySnapshotUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageChangePasswordUseCaseWrapper :
    SmartStorageChangePasswordUseCase
{
    public SmartStorageChangePasswordUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageChangePasswordUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageCreateFolderUseCaseWrapper :
    SmartStorageCreateFolderUseCase
{
    public SmartStorageCreateFolderUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageCreateFolderUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageCreateSnapshotTaskUseCaseWrapper :
    SmartStorageCreateSnapshotTaskUseCase
{
    public SmartStorageCreateSnapshotTaskUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageCreateSnapshotTaskUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageCreateSnapshotUseCaseWrapper :
    SmartStorageCreateSnapshotUseCase
{
    public SmartStorageCreateSnapshotUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageCreateSnapshotUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}
public class SmartStorageDeleteFolderUseCaseWrapper :
    SmartStorageDeleteFolderUseCase
{
    public SmartStorageDeleteFolderUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageDeleteFolderUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageDeleteSnapshotTaskUseCaseWrapper :
    SmartStorageDeleteSnapshotTaskUseCase
{
    public SmartStorageDeleteSnapshotTaskUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageDeleteSnapshotTaskUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageDeleteSnapshotUseCaseWrapper :
    SmartStorageDeleteSnapshotUseCase
{
    public SmartStorageDeleteSnapshotUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageDeleteSnapshotUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageDisableProtocolUseCaseWrapper :
    SmartStorageDisableProtocolUseCase
{
    public SmartStorageDisableProtocolUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageDisableProtocolUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageEnableProtocolUseCaseWrapper :
    SmartStorageEnableProtocolUseCase
{
    public SmartStorageEnableProtocolUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageEnableProtocolUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageRenameUseCaseWrapper :
    SmartStorageRenameUseCase
{
    public SmartStorageRenameUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageRenameUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}

public class SmartStorageUpdateSnapshotTaskUseCaseWrapper :
    SmartStorageUpdateSnapshotTaskUseCase
{
    public SmartStorageUpdateSnapshotTaskUseCaseWrapper(ISmartStoragesService smartStoragesService) :
        base(smartStoragesService)
    {

    }
    public async Task<ServiceResult> Execute(SmartStorageUpdateSnapshotTaskUseCaseRequest request)
    {
        return await base.ExecuteService(request, CancellationToken.None).ConfigureAwait(false);
    }
}