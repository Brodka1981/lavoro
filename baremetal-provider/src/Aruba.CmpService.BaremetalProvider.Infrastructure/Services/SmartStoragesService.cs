using System.Globalization;
using System.Text.RegularExpressions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.SmartStorages.Responses;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.SmartStorages.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;

public class SmartStoragesService :
    BaseResourceService<SmartStorage, SmartStorageProperties, LegacySmartStorageListItem, LegacySmartStorageDetail, ISmartStoragesProvider>,
    ISmartStoragesService
{
    protected override Typologies Typology => Typologies.SmartStorage;
    private readonly ISmartStorageCatalogRepository smartStorageCatalogRepository;

    public SmartStoragesService(
        ILogger<FirewallsService> logger,
        ISmartStoragesProvider smartStoragesProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        ISmartStorageCatalogRepository smartStorageCatalogRepository,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions) : base(logger, smartStoragesProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.smartStorageCatalogRepository = smartStorageCatalogRepository;
    }

    #region SmartStorage
    public async Task<ServiceResult<SmartStorage>> GetById(BaseGetByIdRequest<SmartStorage> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<SmartStorageList>> Search(SmartStorageSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<SmartStorageList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Activate(SmartStorageActivateUseCaseRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await this.ValidateExistence(nameof(Activate), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var legacyResponse = await this.LegacyProvider.ActivateSmartStorage(request.ResourceId, request.ActivateData!.Password!).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "Activate > legacy api call PostSetPassword error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SmartStorageActivate, request.ResourceId!);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "Activate > legacy api call PostSetPassword returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> ChangePassword(SmartStorageChangePasswordUseCaseRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await this.ValidateExistence(nameof(Activate), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var validationPasswordResult = this.ValidatePassword(nameof(ChangePassword), request.ActivateData!.Password, request.ActivateData.ConfirmPassword!);
        if (validationPasswordResult.Errors.Any())
        {
            return validationPasswordResult;
        }

        var legacyResponse = await this.LegacyProvider.ActivateSmartStorage(request.ResourceId, request.ActivateData!.Password!).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "ChangePassword > legacy api call PostSetPassword error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SmartStorageChangePassword, request.ResourceId!);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "ChangePassword > legacy api call PostSetPassword returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }
    #endregion

    #region Automatic Renew

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }

    #endregion

    #region Catalog
    public async Task<ServiceResult<SmartStorageCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        return await this.SearchCatalogInternal<SmartStorageCatalog>(request, cancellationToken).ConfigureAwait(false);
    }
    #endregion

    #region SmartFolders
    public async Task<ServiceResult<IEnumerable<SmartStorageFoldersItem>>> SearchFolders(SmartStorageFoldersRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        var validationResult = await this.ValidateExistence(nameof(SearchFolders), request.UserId, request.ResourceId, request.ProjectId, returnResource: true).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult<IEnumerable<SmartStorageFoldersItem>>()
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var legacyResponse = await this.LegacyProvider.GetSmartFolders(long.Parse(request.ResourceId!, CultureInfo.InvariantCulture)).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(SearchInternal));
            return ServiceResult<IEnumerable<SmartStorageFoldersItem>>.CreateInternalServerError();
        }
        long totalCount = legacyResponse.Result.SmartFolders.Count();
        var smartFolders = await this.MapSmartFolders(legacyResponse.Result.SmartFolders).ConfigureAwait(false);
        return new ServiceResult<IEnumerable<SmartStorageFoldersItem>>()
        {
            Value = smartFolders
        };
    }

    public async Task<ServiceResult<GetAvailableSmartFoldersResponse>> GetAvailableSmartFolders(GetAvailableSmartFoldersRequest request)
    {
        request.ThrowIfNull();
        var validationResult = await this.ValidateExistence(nameof(SearchFolders), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);

        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult<GetAvailableSmartFoldersResponse>
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var id = long.Parse(request.ResourceId!);

        var legacyResponse = await this.LegacyProvider.GetSmartFolders((id)).ConfigureAwait(false);

        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetAvailableSmartFolders));
            return ServiceResult<GetAvailableSmartFoldersResponse>.CreateInternalServerError();
        }

        return new ServiceResult<GetAvailableSmartFoldersResponse>
        {
            Value = new GetAvailableSmartFoldersResponse
            {
                AvailableSmartFolders = legacyResponse.Result.AvailableSmartFolders
            }
        };
    }

    public async Task<ServiceResult> CreateSmartFolder(SmartStorageCreateFolderUseCaseRequest request)
    {
        request.ThrowIfNull();
        var validationResult = await this.ValidateExistence(nameof(EnableProtocol), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }
        var validationNameResult = this.ValidateFolderName(nameof(CreateSmartFolder), request.Name, request.ResourceId);
        if (validationNameResult.Errors.Any())
        {
            return validationNameResult;
        }

        //Validazione nome esistente
        var folders = await this.SearchFolders(new SmartStorageFoldersRequest
        {
            ProjectId = request.ProjectId,
            ResourceId = request.ResourceId,
            UserId = request.UserId
        }, CancellationToken.None).ConfigureAwait(false);
        if (folders.Value?.Any(f => f.Name!.Equals(request.Name, StringComparison.OrdinalIgnoreCase)) ?? false)
        {
            return new ServiceResult().AddError(BadRequestError.Create(FieldNames.Name, "A folder with this name already exists").AddLabel(BaremetalBaseLabelErrors.FolderNameExists(this.Typology)));
        }

        // Create smart folder
        var legacyResponse = await this.LegacyProvider.CreateSmartFolder(request.ResourceId, request.Name!).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "CreateSmartFolder > legacy api call PostCreateSmartFolder error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SmartFolder, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "CreateSmartFolder > legacy api call PostCreateSmartFolder returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }
        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> DeleteSmartFolder(SmartStorageDeleteFolderUseCaseRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(DeleteSnapshot), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        if (request?.SmartFolderId is null)
            return ServiceResult.CreateNotFound(request?.SmartFolderId);
        //Validiamo l'esistenza della folder
        var foldersRequest = new SmartStorageFoldersRequest
        {
            ProjectId = request.ProjectId,
            ResourceId = request.ResourceId,
            UserId = request.UserId
        };
        var folders = await this.SearchFolders(foldersRequest, CancellationToken.None).ConfigureAwait(false);
        var folder = folders.Value!.Where(f => f.Name!.Equals(request.SmartFolderId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        //Verifichiamo se la folder esiste
        if (folder == null)
        {
            return ServiceResult.CreateNotFound(request?.SmartFolderId);
        }
        //
        //Leggiamo gli snapshot associati
        // e li cancelliamo tutti
        var snapshotRequest = new SmartStorageSnapshotsRequest
        {
            ProjectId = request.ProjectId,
            ResourceId = request.ResourceId,
            UserId = request.UserId
        };
        var snapshots = await GetSnapshots(snapshotRequest).ConfigureAwait(false);
        if (snapshots.Value == null)
        {
            Log.LogWarning(Logger, "DeleteSmartFolder > get snapshot call error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateNotFound(request.ResourceId!);
        }
        foreach (var snapshot in snapshots.Value.Snapshots.Where(s => s.SmartFolderName!.Equals(request.SmartFolderId, StringComparison.OrdinalIgnoreCase)))
        {
            var snapshotDeleteRequest = new SmartStorageDeleteSnapshotUseCaseRequest
            {
                ProjectId = request.ProjectId,
                ResourceId = request.ResourceId,
                UserId = request.UserId,
                SnapshotId = snapshot.SnapshotId
            };
            var snapshotResult = await this.DeleteSnapshot(snapshotDeleteRequest).ConfigureAwait(false);
            if (snapshotResult.Errors.Any())
            {
                Log.LogWarning(Logger, "DeleteSmartFolder > delete snapshot call error for smart storage {Id} and snapshot {SnapshotId}", request.ResourceId, snapshot.SnapshotId);
                return snapshotResult;
            }
        }
        foreach (var snapshot in snapshots.Value.SnapshotTasks.Where(s => s.SmartFolderName!.Equals(request.SmartFolderId, StringComparison.OrdinalIgnoreCase)))
        {
            var snapshotDeleteRequest = new SmartStorageDeleteSnapshotTaskUseCaseRequest
            {
                ProjectId = request.ProjectId,
                ResourceId = request.ResourceId,
                UserId = request.UserId,
                SnapshotId = snapshot.SnapshotTaskId
            };
            var snapshotTaskResult = await this.DeleteSnapshotTask(snapshotDeleteRequest).ConfigureAwait(false);
            if (snapshotTaskResult.Errors.Any())
            {
                Log.LogWarning(Logger, "DeleteSmartFolder > delete snapshot task call error for smart storage {Id} and snapshot {SnapshotId}", request.ResourceId, snapshot.SnapshotTaskId);
                return snapshotTaskResult;
            }
        }
        //
        var legacyResponse = await this.LegacyProvider.DeleteSmartFolder(request.ResourceId, folder.SmartFolderID!).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "DeleteSmartFolder > legacy api call PostDeleteSmartFolder error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SmartFolder, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "DeleteSmartFolder > legacy api call PostDeleteSmartFolder returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }
    #endregion

    #region Snapshots
    public async Task<ServiceResult> CreateSnapshot(SmartStorageCreateSnapshotUseCaseRequest request)
    {
        request.ThrowIfNull();

        // Smart folder validation
        var smartFoldersRequest = new SmartStorageFoldersRequest
        {
            ResourceId = request.ResourceId,
            ProjectId = request.ProjectId,
            UserId = request.UserId
        };

        var folders = await this.SearchFolders(smartFoldersRequest, CancellationToken.None).ConfigureAwait(false);
        if (folders.Errors.Any())
        {
            Log.LogInfo(Logger, "CreateSnapshot > search smart folders for smart storage {resourceId} returned {error}", request.ResourceId, folders.Errors.FirstOrDefault()!.ToString());
            return new ServiceResult
            {
                Errors = folders.Errors,
                NotFound = folders.NotFound,
                Envelopes = folders.Envelopes,
            };
        }

        var validationFolderResult = this.ValidateFolderExistence(nameof(CreateSnapshotTask), request.FolderName, folders.Value!);
        if (validationFolderResult.Errors.Any())
        {
            Log.LogInfo(Logger, "CreateSnapshot > validate folder for smart storage {resourceId} validation {error}", request.ResourceId, validationFolderResult.Errors.FirstOrDefault()!.ToString());
            return validationFolderResult;
        }

        // Get snapshots
        var snapshots = await this.LegacyProvider.GetSnapshots(request.ResourceId!).ConfigureAwait(false);
        if (!snapshots.Success || snapshots.Result == null)
        {
            Log.LogWarning(Logger, "GetSnapshots > legacy api call GetSnapshots error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateNotFound(request.ResourceId!);
        }

        // Check snapshot available quantity
        if (snapshots.Result.AvailableSnapshots == 0)
        {
            Log.LogWarning(Logger, "CreateSnapshot > no snapshots available for smart storage {Id}", request.ResourceId);
            return new ServiceResult()
                .AddError(BadRequestError.Create(FieldNames.SnapshotTask, "No available snapshots")
                .AddLabel(BaremetalBaseLabelErrors.SnapshotQuantityExceedsAvailable(this.Typology)));
        }

        // Legacy api call - create snapshot
        var legacyResponse = await this.LegacyProvider.CreateSnapshot(request.ResourceId, request.FolderName!, folders.Value!.FirstOrDefault(f => f.Name!.Equals(request.FolderName, StringComparison.OrdinalIgnoreCase))!.IsRootFolder).ConfigureAwait(false);

        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "CreateSnapshot > legacy api call PostCreateSnapshot error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.Snapshot, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogWarning(Logger, "CreateSnapshot > legacy api call PostCreateSnapshot returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        Log.LogInfo(Logger, "CreateSnapshot > legacy api call PostCreateSnapshot returned success and result true for smart storage {Id}", request.ResourceId);

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);

        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> ApplySnapshot(SmartStorageApplySnapshotUseCaseRequest request)
    {
        request.ThrowIfNull();

        // Get snapshots
        var snapshots = await this.LegacyProvider.GetSnapshots(request.ResourceId!).ConfigureAwait(false);
        if (!snapshots.Success || snapshots.Result == null)
        {
            Log.LogWarning(Logger, "GetSnapshots > legacy api call GetSnapshots error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateNotFound(request.ResourceId!);
        }

        // Legacy api call - restore snapshot
        var legacyResponse = await this.LegacyProvider.RestoreSnapshot(request.ResourceId, request.SnapshotId).ConfigureAwait(false);

        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "CreateSnapshot > legacy api call PostRestoreSnapshot error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.Snapshot, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogWarning(Logger, "CreateSnapshot > legacy api call PostRestoreSnapshot returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        Log.LogInfo(Logger, "CreateSnapshot > legacy api call PostRestoreSnapshot returned success and result true for smart storage {Id}", request.ResourceId);

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);

        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> DeleteSnapshot(SmartStorageDeleteSnapshotUseCaseRequest request)
    {
        // Validation
        var validationResult = await this.ValidateExistence(nameof(DeleteSnapshot), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            Log.LogInfo(Logger, "DeleteSnapshot > smart storage {resourceId}, snapshot {snapshotId} not valid, {errors}", request.ResourceId, request.SnapshotId, validationResult.Errors.FirstOrDefault()?.ToString());

            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        if (string.IsNullOrWhiteSpace(request?.SnapshotId))
        {
            Log.LogWarning(Logger, "DeleteSnapshot > snapshot Id null or empty");
            return ServiceResult.CreateNotFound(request?.SnapshotId);
        }

        // Legacy api call - delete snapshot
        var legacyResponse = await this.LegacyProvider.DeleteSnapshot(request.ResourceId, request.SnapshotId).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "DeleteSnapshot > legacy api call PostDeleteSnapshot error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.Snapshot, request.ResourceId!);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogWarning(Logger, "DeleteSnapshot > legacy api call PostDeleteSnapshot returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        Log.LogInfo(Logger, "DeleteSnapshot > legacy api call PostDeleteSnapshot returned success and result true for smart storage {Id}", request.ResourceId);

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);

        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> CreateSnapshotTask(SmartStorageCreateSnapshotTaskUseCaseRequest request)
    {
        //Recuperiamo l'elenco delle folders e 
        //validiamo l'esistenza della folder presente
        //nell'oggetto di request
        var smartFoldersRequest = new SmartStorageFoldersRequest
        {
            ResourceId = request.ResourceId,
            ProjectId = request.ProjectId,
            UserId = request.UserId
        };
        var folders = await this.SearchFolders(smartFoldersRequest, CancellationToken.None).ConfigureAwait(false);
        if (folders.Errors.Any())
        {
            return new ServiceResult
            {
                Errors = folders.Errors,
                NotFound = folders.NotFound,
                Envelopes = folders.Envelopes,
            };
        }

        var validationFolderResult = this.ValidateFolderExistence(nameof(CreateSnapshotTask), request.SnapshotTask.FolderName!, folders.Value!);
        if (validationFolderResult.Errors.Any())
        {
            return validationFolderResult;
        }

        //Validiamo i parametri inseriti nella request
        //in base al valore di SnapshotLifeTimeUnitType
        var validationResult = this.ValidateSnapshotLifeTimeUnit(nameof(CreateSnapshotTask), request);
        if (validationResult.Errors.Any())
        {
            return validationResult;
        }
        var snapshots = await this.LegacyProvider.GetSnapshots(request.ResourceId!).ConfigureAwait(false);
        if (!snapshots.Success || snapshots.Result == null)
        {
            Log.LogWarning(Logger, "GetSnapshots > legacy api call GetSnapshots error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateNotFound(request.ResourceId!);
        }

        //Verifichiamo che il numero di snapshots disponibile
        //sia minore del numero di iterazioni scelte
        if ((snapshots.Result.AvailableSnapshots < request.SnapshotTask.Quantity) || request.SnapshotTask.Quantity == 0)
        {
            return new ServiceResult().AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Requested snapshot quantity exceeds the available number of snapshots or quantity is 0").AddLabel(BaremetalBaseLabelErrors.SnapshotQuantityExceedsAvailable(this.Typology)));
        }

        var legacyResponse = await this.LegacyProvider.CreateSnapshotTask(request.ResourceId, request.SnapshotTask, folders.Value!.FirstOrDefault(f => f.Name!.Equals(request.SnapshotTask.FolderName, StringComparison.OrdinalIgnoreCase))!.IsRootFolder).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "CreateSnapshotTask > legacy api call PostCreateSnapshotTask error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SmartFolder, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "CreateSnapshotTask > legacy api call PostCreateSnapshotTask returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        Log.LogInfo(Logger, "CreateSnapshotTask > legacy api call PostCreateSnapshotTask returned success and result true for smart storage {Id}", request.ResourceId);

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> UpdateSnapshotTask(SmartStorageUpdateSnapshotTaskUseCaseRequest request)
    {
        request.ThrowIfNull();
        //Recuperiamo l'elenco degli snapshot task
        //validiamo l'esistenza della risorsa e
        //dello snapshot per il quale deve essere
        //modificato lo stato

        var snapshotTasksRequest = new SmartStorageSnapshotsRequest
        {
            ResourceId = request.ResourceId,
            ProjectId = request.ProjectId,
            UserId = request.UserId
        };
        var snapshots = await this.GetSnapshots(snapshotTasksRequest).ConfigureAwait(false);
        if (snapshots.Errors.Any())
        {
            return snapshots;
        }
        if (!snapshots.Value!.SnapshotTasks.Any(st => st.SnapshotTaskId == request.SnapshotId))
        {
            Log.LogWarning(Logger, "UpdateSnapshotTask > snapshot task with Id {SnapshotId} not found for smart storage {Id}", request.SnapshotId, request.ResourceId);
            return ServiceResult.CreateNotFound(request.ResourceId!);
        }

        var legacyResponse = await this.LegacyProvider.UpdateSnapshotTask(request.ResourceId, request.SnapshotId!.Value, enable: request.Enable!.Value).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "EnableProtocol > legacy api call PostEnableService error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.ServiceType!, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "EnableProtocol > legacy api call PostEnableService returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> DeleteSnapshotTask(SmartStorageDeleteSnapshotTaskUseCaseRequest request)
    {
        if (request?.SnapshotId is null)
            return ServiceResult.CreateNotFound(request?.SnapshotId);

        var validationResult = await this.ValidateExistence(nameof(DeleteSnapshotTask), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }


        var legacyResponse = await this.LegacyProvider.DeleteSnapshotTask(request.ResourceId, request.SnapshotId).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "DeleteSnapshotTask > legacy api call PostDeleteSnapshotTask error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.SnapshotTask, request.ResourceId!);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "DeleteSnapshotTask > legacy api call PostDeleteSnapshotTask returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult<SmartStorageSnapshots>> GetSnapshots(SmartStorageSnapshotsRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(GetSnapshots), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult<SmartStorageSnapshots>()
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var legacyResponse = await this.LegacyProvider.GetSnapshots(request.ResourceId!).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetSnapshots > legacy api call GetSnapshots error for smart storage {Id}", request.ResourceId);
            return ServiceResult<SmartStorageSnapshots>.CreateNotFound(request.ResourceId!);
        }

        var ret = new ServiceResult<SmartStorageSnapshots>();

        ret.Value = legacyResponse.Result.MapSnapshots();
        return ret;
    }

    #endregion

    #region Protocols
    public async Task<ServiceResult<IEnumerable<SmartStorageProtocol>>> GetProtocols(SmartStorageProtocolsRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(GetProtocols), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult<IEnumerable<SmartStorageProtocol>>()
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var legacyResponse = await this.LegacyProvider.GetProtolList(request.ResourceId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetProtocols > legacy api call GetServiceStatus error for smart storage {Id}", request.ResourceId);
            return ServiceResult<IEnumerable<SmartStorageProtocol>>.CreateNotFound(request.ResourceId!);
        }

        var ret = new ServiceResult<IEnumerable<SmartStorageProtocol>>();

        ret.Value = legacyResponse.Result.MapProtocolList();
        return ret;
    }

    public async Task<ServiceResult> EnableProtocol(SmartStorageEnableProtocolUseCaseRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(EnableProtocol), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        if (!request.ServiceType.HasValue)
        {
            return new ServiceResult().AddError(BadRequestError.Create(FieldNames.ServiceType, "Servicetype is required").AddLabel(BaremetalBaseLabelErrors.ServiceTypeRequired(this.Typology)));
        }

        var legacyResponse = await this.LegacyProvider.ToggleProtocol(request.ResourceId, request.ServiceType.Value, enable: true).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "EnableProtocol > legacy api call PostEnableService error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.ServiceType!, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "EnableProtocol > legacy api call PostEnableService returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> DisableProtocol(SmartStorageDisableProtocolUseCaseRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(EnableProtocol), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        if (!request.ServiceType.HasValue)
        {
            return new ServiceResult().AddError(BadRequestError.Create(FieldNames.ServiceType, "Servicetype is required").AddLabel(BaremetalBaseLabelErrors.ServiceTypeRequired(this.Typology)));
        }

        var legacyResponse = await this.LegacyProvider.ToggleProtocol(request.ResourceId, request.ServiceType.Value, enable: false).ConfigureAwait(false);
        if (!legacyResponse.Success)
        {
            Log.LogWarning(Logger, "DisableProtocol > legacy api call PostEnableService error for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.ServiceType!, request.ResourceId);
        }
        else if (!legacyResponse.Result)
        {
            Log.LogInfo(Logger, "DisableProtocol > legacy api call PostEnableService returned false for smart storage {Id}", request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }
    #endregion

    #region Statistics
    public async Task<ServiceResult<SmartStorageStatistics>> GetStatistics(SmartStorageStatisticsRequest request)
    {
        var validationResult = await this.ValidateExistence(nameof(GetStatistics), request.UserId, request.ResourceId, request.ProjectId, returnResource: false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return new ServiceResult<SmartStorageStatistics>()
            {
                Errors = validationResult.Errors,
                NotFound = validationResult.NotFound,
                Envelopes = validationResult.Envelopes,
            };
        }

        var legacyResponse = await this.LegacyProvider.GetStatistics(request.ResourceId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "GetStatistics > legacy api call GetStatistics error for smart storage {Id}", request.ResourceId);
            return ServiceResult<SmartStorageStatistics>.CreateNotFound(request.ResourceId!);
        }

        var ret = new ServiceResult<SmartStorageStatistics>();

        ret.Value = legacyResponse.Result.MapStatistics();
        return ret;
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Validate folder existence
    /// </summary>
    private ServiceResult ValidateFolderExistence(string methodName, string? name, IEnumerable<SmartStorageFoldersItem> smartStorageFolders)
    {
        name.ThrowIfNull();
        var ret = new ServiceResult();
        if (!smartStorageFolders.Any(s => s.Name!.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            Log.LogWarning(Logger, "{MethodName} > The folder not exists");
            ret.AddError(BadRequestError.Create(FieldNames.SmartFolder, "The folder name not exists.").AddLabel(BaremetalBaseLabelErrors.FolderNotExists(this.Typology)));
            return ret;
        }
        return ret;
    }

    /// <summary>
    /// Validate Snapshot LifeTime Unit
    /// </summary>
    private ServiceResult ValidateSnapshotLifeTimeUnit(string methodName, SmartStorageCreateSnapshotTaskUseCaseRequest request)
    {
        request.ThrowIfNull();
        var ret = new ServiceResult();
        if (!request.SnapshotTask.LifeTimeUnitType.HasValue)
        {
            Log.LogWarning(Logger, "{MethodName} > Snapshot Task LifeTimeUnitType is required");
            ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Snapshot Task LifeTimeUnitType is required.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitTypeRequired(this.Typology)));
            return ret;
        }

        switch (request.SnapshotTask.LifeTimeUnitType)
        {
            case SnapshotLifeTimeUnitTypes.Hourly:
                if (!request.SnapshotTask.Minute.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Minutes must be specified for Hourly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Minutes must be specified for Hourly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitHourlyCreationTimeRequired(this.Typology)));
                    return ret;
                }
                if (request.SnapshotTask.Hour.HasValue || request.SnapshotTask.DaysOfMonth.HasValue || request.SnapshotTask.Month.HasValue || request.SnapshotTask.DayOfWeek.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Only Minutes should be specified for Hourly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Only Minutes should be specified for Hourly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitInvalidHourlyParameters(this.Typology)));
                    return ret;
                }
                break;
            case SnapshotLifeTimeUnitTypes.Daily:
                if (!request.SnapshotTask.Hour.HasValue || !request.SnapshotTask.Minute.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Both Hours and Minutes must be specified for Daily snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Both Hours and Minutes must be specified for Daily snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitDailyCreationTimeRequired(this.Typology)));
                    return ret;
                }

                if (request.SnapshotTask.DaysOfMonth.HasValue || request.SnapshotTask.Month.HasValue || request.SnapshotTask.DayOfWeek.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Only Hours and Minutes should be specified for Daily snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Only Hours and Minutes should be specified for Daily snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitInvalidDailyParameters(this.Typology)));
                    return ret;
                }
                break;

            case SnapshotLifeTimeUnitTypes.Weekly:
                if (!request.SnapshotTask.DayOfWeek.HasValue || !request.SnapshotTask.Hour.HasValue || !request.SnapshotTask.Minute.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > DayOfWeek, Hours and Minutes must be specified for Weekly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "DayOfWeek, Hours and Minutes must be specified for Weekly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitWeeklyCreationTimeAndDayOfWeekRequired(this.Typology)));
                    return ret;
                }
                if (request.SnapshotTask.DaysOfMonth.HasValue || request.SnapshotTask.Month.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Only DayOfWeek, Hours and Minutes should be specified for Weekly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Only DayOfWeek, Hours and Minutes should be specified for Weekly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitInvalidWeeklyParameters(this.Typology)));
                    return ret;
                }
                break;

            case SnapshotLifeTimeUnitTypes.Monthly:
                if (!request.SnapshotTask.DaysOfMonth.HasValue || !request.SnapshotTask.Hour.HasValue || !request.SnapshotTask.Minute.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > DaysOfMonth, Hours and Minutes must be specified for Monthly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "DaysOfMonth, Hours and Minutes must be specified for Monthly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitMonthlyCreationTimeAndDayOfMonthRequired(this.Typology)));
                    return ret;
                }

                if (request.SnapshotTask.Month.HasValue || request.SnapshotTask.DayOfWeek.HasValue)
                {
                    Log.LogWarning(Logger, "{MethodName} > Only DaysOfMonth, Hours and Minutes should be specified for Monthly snapshots tasks");
                    ret.AddError(BadRequestError.Create(FieldNames.SnapshotTask, "Only DaysOfMonth, Hours and Minutes should be specified for Monthly snapshots tasks.").AddLabel(BaremetalBaseLabelErrors.SnapshotLifeTimeUnitInvalidMonthlyParameters(this.Typology)));
                    return ret;
                }
                break;
        }

        return ret;
    }

    /// <summary>
    /// Validate name
    /// </summary>
    private ServiceResult ValidateFolderName(string methodName, string? name, string id)
    {
        name.ThrowIfNull();
        var ret = new ServiceResult();
        var specialChars = "_-. ".ToCharArray();
        var invalidChar = name!.Where(f => !char.IsNumber(f) && !char.IsLetter(f) && !specialChars.Contains(f)).ToList();
        string pattern = @"^(?! )[a-zA-Z0-9_\-. ]+(?<! )$";
        if (name.Length < 4)
        {
            Log.LogWarning(Logger, "{MethodName} > The folder name must have at least one character");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "The folder name must have at least one character.").AddLabel(BaremetalBaseLabelErrors.FolderNameMinimumLength(this.Typology)));

            return ret;
        }
        if (name.Length > 100)
        {
            Log.LogWarning(Logger, "{MethodName} > The folder name must have a maximum of 100 alphanumeric characters");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "The folder name must have a maximum of 100 alphanumeric characters.").AddLabel(BaremetalBaseLabelErrors.FolderNameMaximumLength(this.Typology)));
            return ret;
        }

        if (!Regex.IsMatch(name!, pattern, RegexOptions.None, TimeSpan.FromMilliseconds(500)))
        {
            Log.LogWarning(Logger, "{MethodName} > The folder name contains illegal characters");
            if (invalidChar.Count > 0)
            {
                ret.AddError(BadRequestError.Create(FieldNames.Name, $"The folder name has invalid chars:{string.Join("", invalidChar)}").AddLabel(BaremetalBaseLabelErrors.FolderNameInvalidChars(this.Typology)).AddParam("char", invalidChar));
            }
            else
            {
                ret.AddError(BadRequestError.Create(FieldNames.Name, $"The folder name cannot start or end with a space").AddLabel(BaremetalBaseLabelErrors.FolderNameInvalidChars(this.Typology)).AddParam("char", "space"));
            }

            return ret;
        }
        return ret;
    }

    /// <summary>
    /// Validate password matches confirm password 
    /// </summary>
    private ServiceResult ValidatePassword(string methodName, string? password, string repeatPassword)
    {
        password.ThrowIfNull();
        var ret = new ServiceResult();
        if (!password.Equals(repeatPassword, StringComparison.OrdinalIgnoreCase))
        {
            Log.LogWarning(Logger, "{MethodName} > Password and Confirm Password do not match.");
            ret.AddError(BadRequestError.Create(FieldNames.SmartStorageChangePassword, "Password and Confirm Password do not match..").AddLabel(BaremetalBaseLabelErrors.PasswordAndConfirmDoNotMatch(this.Typology)));

            return ret;
        }
        return ret;
    }

    private async Task<IEnumerable<SmartStorageFoldersItem>> MapSmartFolders(IEnumerable<LegacySmartFoldersItem> items)
    {
        var ret = items.MapToSmartStorageFolders().ToList();
        return await Task.FromResult(ret).ConfigureAwait(false);
    }
    #endregion

    #region Overrides
    protected override async Task<SmartStorage> MapToListItem(LegacySmartStorageListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        return resource.MapToListitem(userId, project);
    }

    protected override async Task<SmartStorage> MapToDetail(LegacySmartStorageDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(userId, project, profile!.IsResellerCustomer!.Value);
    }

    protected override DateTimeOffset? GetDueDate(SmartStorage resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override async Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string language)
    {
        var smartStorageCatalog = await this.smartStorageCatalogRepository.GetAllAsync().ConfigureAwait(false);
        var ret = new SmartStorageCatalog()
        {
            TotalCount = totalCount,
            Values = items.MapToSmartStorageCatalog(smartStorageCatalog).ToList()
        };

        return await Task.FromResult(ret).ConfigureAwait(false);
    }

    protected override IResourceMessage GetUpdatedEvent(SmartStorage smartStorage, AutorenewFolderAction? action)
    {
        return new SmartStorageUpdatedDeployment()
        {
            Resource = smartStorage.Map(action)
        };
    }
    #endregion
}
