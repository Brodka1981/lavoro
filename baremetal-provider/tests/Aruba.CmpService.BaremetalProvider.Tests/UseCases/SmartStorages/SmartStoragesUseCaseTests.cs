using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using FluentAssertions;
using Moq;
using NSubstitute;

namespace Aruba.CmpService.BaremetalProvider.Tests.UseCases.SmartStorages;

public class SmartStoragesUseCaseTests : TestBase
{
    public SmartStoragesUseCaseTests(ITestOutputHelper output) : base(output) { }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        var smartStoragesService = Substitute.For<ISmartStoragesService>();
        services.AddSingleton(smartStoragesService);
        services.AddSingleton<SmartStorageActivateUseCaseWrapper>();
        services.AddSingleton<SmartStorageApplySnapshotUseCaseWrapper>();
        services.AddSingleton<SmartStorageChangePasswordUseCaseWrapper>();
        services.AddSingleton<SmartStorageCreateFolderUseCaseWrapper>();
        services.AddSingleton<SmartStorageCreateSnapshotTaskUseCaseWrapper>();
        services.AddSingleton<SmartStorageCreateSnapshotUseCaseWrapper>();
        services.AddSingleton<SmartStorageDeleteFolderUseCaseWrapper>();
        services.AddSingleton<SmartStorageDeleteSnapshotTaskUseCaseWrapper>();
        services.AddSingleton<SmartStorageDeleteSnapshotUseCaseWrapper>();
        services.AddSingleton<SmartStorageDisableProtocolUseCaseWrapper>();
        services.AddSingleton<SmartStorageEnableProtocolUseCaseWrapper>();
        services.AddSingleton<SmartStorageRenameUseCaseWrapper>();
        services.AddSingleton<SmartStorageUpdateSnapshotTaskUseCaseWrapper>();
    }

    [Fact]
    [Unit]
    public async Task Activate_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.Activate(It.IsAny<SmartStorageActivateUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageActivateUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageActivateUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task ApplySnapshot_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.ApplySnapshot(It.IsAny<SmartStorageApplySnapshotUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageApplySnapshotUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageApplySnapshotUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task ChangePassword_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.ChangePassword(It.IsAny<SmartStorageChangePasswordUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageChangePasswordUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageChangePasswordUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task CreateSmartFolder_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.CreateSmartFolder(It.IsAny<SmartStorageCreateFolderUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageCreateFolderUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageCreateFolderUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshotTask_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.CreateSnapshotTask(It.IsAny<SmartStorageCreateSnapshotTaskUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageCreateSnapshotTaskUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageCreateSnapshotTaskUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task CreateSnapshot_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.CreateSnapshot(It.IsAny<SmartStorageCreateSnapshotUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageCreateSnapshotUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageCreateSnapshotUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DeleteSmartFolder_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.DeleteSmartFolder(It.IsAny<SmartStorageDeleteFolderUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageDeleteFolderUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageDeleteFolderUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshotTask_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.DeleteSnapshotTask(It.IsAny<SmartStorageDeleteSnapshotTaskUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageDeleteSnapshotTaskUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageDeleteSnapshotTaskUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DeleteSnapshot_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.DeleteSnapshot(It.IsAny<SmartStorageDeleteSnapshotUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageDeleteSnapshotUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageDeleteSnapshotUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task DisableProtocol_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.DisableProtocol(It.IsAny<SmartStorageDisableProtocolUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageDisableProtocolUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageDisableProtocolUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task EnableProtocol_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.EnableProtocol(It.IsAny<SmartStorageEnableProtocolUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageEnableProtocolUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageEnableProtocolUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task Rename_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.Rename(It.IsAny<SmartStorageRenameUseCaseRequest>(), It.IsAny<CancellationToken>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageRenameUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageRenameUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }

    [Fact]
    [Unit]
    public async Task UpdateSnapshotTask_Test()
    {
        var provider = CreateServiceCollection().BuildServiceProvider();

        var smartStoragesService = provider.GetRequiredService<ISmartStoragesService>();
        smartStoragesService.UpdateSnapshotTask(It.IsAny<SmartStorageUpdateSnapshotTaskUseCaseRequest>()).ReturnsForAnyArgs(new ServiceResult());

        var useCase = provider.GetRequiredService<SmartStorageUpdateSnapshotTaskUseCaseWrapper>();
        var ret = await useCase.Execute(new SmartStorageUpdateSnapshotTaskUseCaseRequest()).ConfigureAwait(false);
        ret.Errors.Should().HaveCount(0);
    }
}
