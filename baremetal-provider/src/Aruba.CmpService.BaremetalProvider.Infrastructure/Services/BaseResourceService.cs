using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Payments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Aruba.MessageBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;


namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public abstract class BaseResourceService<TResource, TProperties, TLegacyListItem, TLegacyDetail, TLegacyProvider>
    where TResource : ResourceBase<TProperties>
    where TProperties : class, IResourceProperties
    where TLegacyDetail : LegacyResourceDetail
    where TLegacyListItem : LegacyResourceListItem
    where TLegacyProvider : ILegacyProvider<TLegacyListItem, TLegacyDetail>
{
    protected abstract Typologies Typology { get; }
    protected ILogger Logger { get; }
    protected TLegacyProvider LegacyProvider { get; }
    protected IProjectProvider ProjectProvider { get; }
    protected ILocationMapRepository LocationMapRepository { get; }
    protected RenewFrequencyOptions RenewFrequencyOptions { get; }
    protected int SSDTimeLimit { get; }
    protected ICatalogueProvider CatalogueProvider { get; }
    protected IPaymentsProvider PaymentsProvider { get; }
    protected IProfileProvider ProfileProvider { get; }
    protected bool EnableUpdatedEvent { get; }
    protected IMessageBus MessageBus { get; }

    protected BaseResourceService(
        ILogger logger,
        TLegacyProvider legacyProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        ICatalogueProvider catalogueProvider,
        IPaymentsProvider paymentsProvider,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions)
    {
        Logger = logger;
        LegacyProvider = legacyProvider;
        ProjectProvider = projectProvider;
        LocationMapRepository = locationMapRepository;
        RenewFrequencyOptions = renewFrequencyOptions!.Value;
        CatalogueProvider = catalogueProvider;
        PaymentsProvider = paymentsProvider;
        ProfileProvider = profileProvider;
        SSDTimeLimit = baremetalOptions.Value.SddTimeLimit ?? 0;
        EnableUpdatedEvent = enableUpdatedEventOptions!.Value.Enable;
    }

    #region Read Methods
    protected async Task<ServiceResult<TResource>> GetByIdInternal(string? resourceId, string? userId, string? projectId, CancellationToken cancellationToken)
    {
        var ret = await this.ValidateExistence(nameof(GetByIdInternal), userId, resourceId, projectId, true).ConfigureAwait(false);
        return ret;
    }

    protected async Task<ServiceResult<T>> SearchInternal<T>(BaseSearchFiltersRequest<TResource> request, CancellationToken cancellationToken) where T : ListResponse<TResource>
    {
        request.ThrowIfNull();

        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            Log.LogWarning(Logger, "{MethodName} > missing userId", nameof(SearchInternal));
            return ServiceResult<T>.CreateForbiddenError();
        }
        if (string.IsNullOrWhiteSpace(request.ProjectId))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(SearchInternal), request.ProjectId);
            return ServiceResult<T>.CreateNotFound(request.ProjectId);
        }
        var projectResponse = await this.ProjectProvider.GetProjectAsync(request.UserId!, request.ProjectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(SearchInternal), request.ProjectId);
            return ServiceResult<T>.CreateNotFound(request.ProjectId);
        }
        //Leggo le risorse
        var filterRequest = new LegacySearchFilters()
        {
            External = request.External,
            Query = request.Query
        };
        var legacyResponse = await this.LegacyProvider.Search(filterRequest).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(SearchInternal));
            return ServiceResult<T>.CreateInternalServerError();
        }

        // Filters
        foreach (var filter in request.Query?.Filters ?? new FilterDefinitionsList())
        {
            switch (filter)
            {
                case var f when f.IsFilterFor("ids".AsField<string>(), op => op.In, out var arg):

                    legacyResponse.Result.Items = legacyResponse.Result.Items
                        .Where(i => arg.Contains(i.Id.ToString())).ToList();

                    legacyResponse.Result.TotalItems = legacyResponse.Result.Items.Count();
                    break;
            }
        }

        var value = Activator.CreateInstance<T>();

        value.TotalCount = legacyResponse.Result.TotalItems;

        foreach (var item in legacyResponse.Result.Items)
        {

            value.Values.Add(await this.MapToListItem(item, request.UserId, projectResponse.Result).ConfigureAwait(false));
        }

        var ret = new ServiceResult<T>()
        {
            Value = value
        };

        return ret;
    }

    protected async Task<ServiceResult<T>> SearchCatalogInternal<T>(CatalogFilterRequest request, CancellationToken cancellationToken) where T : ICatalog
    {
        request.ThrowIfNull();

        var legacyResponse = await this.LegacyProvider.GetCatalog().ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(SearchInternal));
            return ServiceResult<T>.CreateInternalServerError();
        }
        var catalogItems = legacyResponse.Result.SelectMany(sm => sm.Elements, (source, item) =>
        {
            item.Category = source.Code;
            item.BaseConfigProducts = item.BaseConfigProducts ?? new List<LegacyCatalogItemConfigProduct>();
            return item;
        }).ToList();
        long totalCount = catalogItems.Count;
        var catalog = await this.MapCatalog(catalogItems, totalCount, request.Language).ConfigureAwait(false);
        return new ServiceResult<T>()
        {
            Value = (T)catalog
        };
    }
    #endregion

    #region WriteMethods
    protected virtual async Task<ServiceResult> RenameInternal(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        var validationResult = await this.ValidateExistence(nameof(RenameInternal), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);
        if (!validationResult.ContinueCheckErrors)
        {
            return validationResult;
        }
        var server = validationResult.Value;
        var nameValidationResult = ValidateName(nameof(RenameInternal), request.RenameData?.Name, request.ResourceId);
        if (nameValidationResult.Errors.Any())
        {
            return nameValidationResult;
        }

        var renameData = new ResourceRename()
        {
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
            CustomName = request.RenameData?.Name

        };
        var resultResponse = await this.LegacyProvider.Rename(renameData).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            return ServiceResult<TResource>.CreateLegacyError(resultResponse, this.Typology, FieldNames.Name, request.ResourceId!);
        }
        else if (!resultResponse.Result)
        {
            return ServiceResult<TResource>.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };

    }

    protected virtual async Task<ServiceResult> UpsertAutomaticRenewInternal(UpsertAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        Log.LogInfo(Logger, "UpsertAutomaticRenewInternal - before validate");

        var ret = await this.ValidateExistence(nameof(UpsertAutomaticRenewInternal), request.UserId, request.ResourceId, request.ProjectId, true).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            return ret;
        }

        if (string.IsNullOrWhiteSpace(request.RenewData?.PaymentMethodId))
        {
            Log.LogWarning(Logger, "{MethodName} > PaymentId is required");
            ret.AddError(BadRequestError.Create(FieldNames.PaymentId, "PaymentId is required.").AddLabel(BaremetalBaseLabelErrors.PaymentMethodIdRequired(this.Typology)));
        }

        if (!(request.RenewData?.Months.HasValue ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > Months is required");
            ret.AddError(BadRequestError.Create(FieldNames.Months, "Months is required.").AddLabel(BaremetalBaseLabelErrors.MonthsRequired(this.Typology)));
        }
        else if (!RenewFrequencyOptions.Contains(request.RenewData!.Months!.Value))
        {
            var validMonthsForLog = string.Join(",", RenewFrequencyOptions.Select(s => s.ToString(CultureInfo.InvariantCulture)));
            Log.LogWarning(Logger, "{MethodName} > Months must be " + validMonthsForLog);
            ret.AddError(BadRequestError.Create(FieldNames.Months, $"Months must be {validMonthsForLog}.").AddLabel(BaremetalBaseLabelErrors.MonthsInvalid(this.Typology)));
        }
        if (ret.Errors.Any())
        {
            return ret;
        }
        var legacyFraudRiskAssessmentResponse = await PaymentsProvider.GetFraudRiskAssessmentAsync().ConfigureAwait(false);
        if (!legacyFraudRiskAssessmentResponse.Success)
        {
            Log.LogWarning(Logger, "{MethodName} > {legacyResponse}", nameof(UpsertAutomaticRenewInternal), legacyFraudRiskAssessmentResponse.Serialize());
            return ServiceResult<TResource>.CreateInternalServerError();
        }
        if (legacyFraudRiskAssessmentResponse.Result)
        {
            Log.LogWarning(Logger, "{MethodName} > Fraud Risk");
            ret.AddError(BadRequestError.Create(FieldNames.AutomaticRenew, "An error has occurred.").AddLabel(BaremetalBaseLabelErrors.FraudRisk(this.Typology)));
        }
        var paymentData = request.RenewData!.PaymentMethodId.CreateInfo();
        if (!ValidatePaymentMethod(ret.Value!, paymentData!.DeviceType))
        {
            Log.LogWarning(Logger, "{MethodName} > Sdd not allowed");
            ret.AddError(BadRequestError.Create(FieldNames.AutomaticRenew, "Sdd not allowed.").AddLabel(BaremetalBaseLabelErrors.SddNotAllowed(this.Typology)));
        }
        if (ret.Errors.Any())
        {
            return ret;
        }
        Log.LogInfo(Logger, "UpsertAutomaticRenewInternal - before prepara httpcall");
        var renewData = new ResourceUpsertAutomaticRenew()
        {
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
            FrequencyDuration = request.RenewData.Months,
            DeviceId = paymentData!.DeviceId,
            DeviceType = (long)paymentData.DeviceType!
        };
        var resultResponse = await this.LegacyProvider.UpsertAutomaticRenew(renewData).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            Log.LogInfo(Logger, "UpsertAutomaticRenewInternal - renew Failed");
            return ServiceResult<TResource>.CreateLegacyError(resultResponse, this.Typology, FieldNames.AutomaticRenew, request.ResourceId!);
        }
        else if (!resultResponse.Result)
        {
            Log.LogInfo(Logger, "UpsertAutomaticRenewInternal - result false");
            return ServiceResult<TResource>.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);

        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };

    }

    protected virtual async Task<ServiceResult<TResource>> DeleteAutomaticRenewInternal(DeleteAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        var ret = await this.ValidateExistence(nameof(DeleteAutomaticRenewInternal), request.UserId, request.ResourceId, request.ProjectId, true).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            return ret;
        }

        var renewData = new ResourceDeleteAutomaticRenew()
        {
            Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
        };
        var resultResponse = await this.LegacyProvider.DeleteAutomaticRenew(renewData).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            return ServiceResult<TResource>.CreateLegacyError(resultResponse, this.Typology, FieldNames.AutomaticRenew, request.ResourceId!);
        }
        else if (!resultResponse.Result)
        {
            return ServiceResult<TResource>.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);

        return new ServiceResult<TResource>()
        {
            Value = ret.Value,
            Envelopes = new[] { signalREnvelope },
        };

    }

    protected async Task<Location?> GetLocation(string? legacyCode)
    {
        if (!string.IsNullOrWhiteSpace(legacyCode))
        {
            var dbLocation = await this.LocationMapRepository.GetLocationMapAsync(legacyCode).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(dbLocation?.Value))
            {
                var locationResponse = await this.CatalogueProvider.GetLocationByValue(dbLocation.Value).ConfigureAwait(false);
                if (locationResponse.Success && locationResponse.Result != null)
                {
                    return new Location()
                    {
                        City = locationResponse.Result.City,
                        Country = locationResponse.Result.Country,
                        Code = locationResponse.Result.Abbreviation,
                        Name = locationResponse.Result.CrowdInName,
                        Value = locationResponse.Result.Value
                    };
                }
            }
        }
        return null;
    }

    protected async Task<UserProfile?> GetProfile(string userId)
    {
        var profileResponse = await this.ProfileProvider.GetUser(userId).ConfigureAwait(false);
        if (profileResponse.Success && profileResponse.Result != null)
        {
            return new UserProfile()
            {
                IsResellerCustomer = profileResponse.Result.IsResellerCustomer
            };
        }
        return null;
    }

    protected virtual async Task<ServiceResult> SetAutomaticRenewInternal([NotNull] SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        if (request.Activate)
        {
            var upsertRequest = new UpsertAutomaticRenewUseCaseRequest()
            {
                ProjectId = request.ProjectId,
                RenewData = new Abstractions.Dtos.UpsertAutomaticRenewDto()
                {
                    PaymentMethodId = request.PaymentMethodId,
                    Months = request.Months,
                },
                UserId = request.UserId,
                ResourceId = request.ResourceId,
            };
            var ret = await this.UpsertAutomaticRenewInternal(upsertRequest, cancellationToken).ConfigureAwait(false);

            if (ret.Errors.Any())
            {
                return ret;
            }
            return ret;
        }
        else
        {
            // DISABLE AUTORENEW
            var deleteRequest = new DeleteAutomaticRenewUseCaseRequest()
            {
                ProjectId = request.ProjectId,
                UserId = request.UserId,
                ResourceId = request.ResourceId,

            };
            var ret = await this.DeleteAutomaticRenewInternal(deleteRequest, cancellationToken).ConfigureAwait(false);

            if (ret.Errors.Any())
            {
                return ret;
            }

            if (EnableUpdatedEvent)
            {
                // SEND UPDATED EVENT
                var message = this.GetUpdatedEvent(ret.Value!, request.ActionOnFolder);
                ret.AddEnvelopes(MessageUtils.EnvelopeCreateWithSubject(subject: message.DeploymentId!, body: message));
            }

            return ret;
        }
    }

    #endregion 

    #region base validations
    /// <summary>
    /// Validate name
    /// </summary>
    protected ServiceResult ValidateName(string methodName, string? name, string id)
    {
        var ret = new ServiceResult();
        if (string.IsNullOrWhiteSpace(name))
        {
            Log.LogWarning(Logger, "{MethodName} > Resource name is required");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "Resource name is required.").AddLabel(BaremetalBaseLabelErrors.NameRequired(this.Typology)));
            return ret;
        }

        if (name.Length < 4)
        {
            Log.LogWarning(Logger, "{MethodName} > The resource name must have at least four character");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "The resource name must have at least four character.").AddLabel(BaremetalBaseLabelErrors.NameMinimumLength(this.Typology)));

            return ret;
        }
        if (name.Length > 50)
        {
            Log.LogWarning(Logger, "{MethodName} > The resource name must have a maximum of 50 alphanumeric characters");
            ret.AddError(BadRequestError.Create(FieldNames.Name, "The resource name must have a maximum of 50 alphanumeric characters.").AddLabel(BaremetalBaseLabelErrors.NameMaximumLength(this.Typology)));
            return ret;
        }

        var specialChars = ".,;:?!@#$%^&*\\_~'()-/+".ToCharArray();

        var invalidChar = name.Where(f => !char.IsNumber(f) && !char.IsLetter(f) && !specialChars.Contains(f) && !char.IsWhiteSpace(f)).ToList();

        if (invalidChar.Count > 0)
        {
            Log.LogWarning(Logger, "{MethodName} > The resource name contains illegal characters");
            ret.AddError(BadRequestError.Create(FieldNames.Name, $"The resource name has invalid chars:{invalidChar}").AddLabel(BaremetalBaseLabelErrors.NameInvalidChars(this.Typology)).AddParam("char", invalidChar));
            return ret;
        }
        return ret;
    }

    /// <summary>
    /// Validate existence and user
    /// </summary>
    protected async Task<ServiceResult<TResource>> ValidateExistence(string methodName, string? userId, string? resourceId, string? projectId, bool returnResource)
    {
        var ret = new ServiceResult<TResource>();

        if (string.IsNullOrWhiteSpace(userId))
        {
            Log.LogWarning(Logger, "{MethodName} > missing userId", methodName);
            return ServiceResult<TResource>.CreateForbiddenError();
        }
        if (string.IsNullOrWhiteSpace(projectId))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", methodName, projectId);
            return ServiceResult<TResource>.CreateNotFound(projectId);
        }
        var projectResponse = await this.ProjectProvider.GetProjectAsync(userId!, projectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", methodName, projectId);
            return ServiceResult<TResource>.CreateNotFound(projectId);
        }

        if (string.IsNullOrWhiteSpace(resourceId))
        {
            Log.LogWarning(Logger, "{MethodName} > resource with Id {Id} not found", methodName, resourceId);
            return ServiceResult<TResource>.CreateNotFound(resourceId);
        }

        if (!long.TryParse(resourceId, out var id))
        {
            Log.LogWarning(Logger, "{MethodName} > resource with Id {Id} not found", methodName, resourceId);
            return ServiceResult<TResource>.CreateNotFound(resourceId);
        }

        if (returnResource)
        {
            var legacyResponse = await this.LegacyProvider.GetById(id).ConfigureAwait(false);
            if (!legacyResponse.Success || legacyResponse.Result == null)
            {
                Log.LogWarning(Logger, "{MethodName} > resource with Id {Id} not found", methodName, resourceId);
                return ServiceResult<TResource>.CreateNotFound(resourceId!);
            }
            ret.Value = await this.MapToDetail(legacyResponse.Result!, userId!, projectResponse.Result).ConfigureAwait(false);
            return ret;

        }
        return ret;
    }

    protected async Task<ServiceResult> ValidateUserAndProject(string methodName, string? userId, string? projectId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            Log.LogWarning(Logger, "{MethodName} > missing userId", methodName);
            return ServiceResult.CreateForbiddenError();
        }
        if (string.IsNullOrWhiteSpace(projectId))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", methodName, projectId);
            return ServiceResult.CreateNotFound(projectId);
        }
        var projectResponse = await this.ProjectProvider.GetProjectAsync(userId!, projectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", methodName, projectId);
            return ServiceResult.CreateNotFound(projectId);
        }
        return new ServiceResult();
    }
    private bool ValidatePaymentMethod(TResource resource, LegacyPaymentType payment)
    {
        payment.ThrowIfNull();
        if (payment == LegacyPaymentType.Sdd)
        {
            var dueDate = this.GetDueDate(resource);
            if (dueDate.HasValue && dueDate.Value.AddDays(-this.SSDTimeLimit) < DateTimeOffset.UtcNow)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region abstract Methods
    protected abstract Task<TResource> MapToListItem(TLegacyListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project);
    protected abstract Task<TResource> MapToDetail(TLegacyDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project);
    protected abstract Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null);
    protected abstract DateTimeOffset? GetDueDate(TResource resource);
    protected abstract IResourceMessage GetUpdatedEvent(TResource resource, AutorenewFolderAction? action);
    #endregion
}
