using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Servers.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.MessageBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;
using ServerCatalog = Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers.ServerCatalog;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class ServersService :
    BaseResourceService<Server, ServerProperties, LegacyServerListItem, LegacyServerDetail, IServersProvider>,
    IServersService
{
    private readonly IServersProvider serversProvider;
    private readonly IServerCatalogRepository serverCatalogRepository;

    protected override Typologies Typology => Typologies.Server;

    public ServersService(
        ILogger<ServersService> logger,
        IServersProvider serversProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IServerCatalogRepository serverCatalogRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions) : base(logger, serversProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.serversProvider = serversProvider;
        this.serverCatalogRepository = serverCatalogRepository;
    }

    public async Task<ServiceResult<ServerList>> Search([NotNull] ServerSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await this.SearchInternal<ServerList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<Server>> GetById([NotNull] BaseGetByIdRequest<Server> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Rename([NotNull] RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var res = await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
        if (res.Errors.Any())
        {
            return new ServiceResult<Server>()
            {
                Errors = res.Errors,
            };
        }
        return res;
    }

    public async Task<ServiceResult> Restart([NotNull] ServerRestartUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        var serverValidationResult = await this.ValidateExistence(nameof(Restart), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);
        if (!serverValidationResult.ContinueCheckErrors)
        {
            return serverValidationResult;
        }

        var resultResponse = await this.LegacyProvider.Restart(long.Parse(request.ResourceId, CultureInfo.InvariantCulture)).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            return ServiceResult.CreateLegacyError(resultResponse, this.Typology, FieldNames.Restart, request.ResourceId!);
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult> UpdateIpAddress([NotNull] UpdateIpUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.IpAddress.ThrowIfNull();

        var serverValidationResult = await this.ValidateExistence(nameof(UpdateIpAddress), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);
        if (!serverValidationResult.ContinueCheckErrors)
        {
            Log.LogInfo(Logger, "{MethodName} > ValidateUserAndProject failed {error} for {resourceId}", nameof(UpdateIpAddress), serverValidationResult.Serialize(), request.ResourceId);
            return serverValidationResult;
        }

        var data = request.Map();
        var resultResponse = await this.LegacyProvider.UpdateIpAddress(data).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            Log.LogWarning(Logger, "{MethodName} > Legacy UpdateIpAddress not successful for {resourceId}", nameof(UpdateIpAddress), request.ResourceId);
            return ServiceResult.CreateLegacyError(resultResponse, Typology, FieldNames.IpAddress, request.Id.ToString(CultureInfo.InvariantCulture));
        }
        if (!resultResponse.Result)
        {
            Log.LogWarning(Logger, "{MethodName} > Legacy UpdateIpAddress error for {resourceId}", nameof(UpdateIpAddress), request.ResourceId);
            return ServiceResult.CreateInternalServerError();
        }

        Log.LogInfo(Logger, "{MethodName} > Legacy UpdateIpAddress successful for {resourceId}", nameof(UpdateIpAddress), request.ResourceId);

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult<ServerIpAddressList>> SearchIpAddresses(ServerIpAddressesFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        var ret = await this.ValidateUserAndProject(nameof(SearchIpAddresses), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            Log.LogInfo(Logger, "{MethodName} > ValidateUserAndProject failed {error} for {resourceId}", nameof(SearchIpAddresses), ret.Serialize(), request.ResourceId);
            return ServiceResult<ServerIpAddressList>.FromBase(ret);
        }
        var filterRequest = request.Map();

        var legacyResponse = await this.serversProvider.SearchIpAddresses(filterRequest, request.ResourceId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            if (legacyResponse.Err?.Code?.Equals("ERR_CLOUDDCS_REVERSEDNS_NOTFOUND", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                Log.LogInfo(Logger, "{MethodName} > resource retrieve not found error for {resourceId}", nameof(SearchIpAddresses), request.ResourceId);
                return new ServiceResult<ServerIpAddressList>()
                {
                    Value = new LegacyListResponse<LegacyIpAddress>() { TotalItems = 0 }.Map()
                };
            }
            else
            {
                Log.LogWarning(Logger, "{MethodName} > resource retrieve errors for {resourceId}", nameof(SearchIpAddresses), request.ResourceId);

                return ServiceResult<ServerIpAddressList>.CreateInternalServerError();
            }
        }

        Log.LogInfo(Logger, "{MethodName} > successful for {resourceId}", nameof(SearchIpAddresses), request.ResourceId);
        return new ServiceResult<ServerIpAddressList>()
        {
            Value = legacyResponse.Result.Map()
        };
    }

    public async Task<ServiceResult> DeletePleskLicense([NotNull] DeletePleskLicenseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        var server = await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
        if (!server.ContinueCheckErrors)
        {
            return server;
        }

        if (!string.IsNullOrWhiteSpace(server.Value?.Properties?.PleskLicense?.Code))
        {
            var deletePleskLicense = new DeletePleskLicense()
            {
                Id = long.Parse(request.ResourceId, CultureInfo.InvariantCulture),
                LicenseCode = server.Value.Properties.PleskLicense.Code
            };
            var legacyResponse = await this.serversProvider.DeletePleskLicense(deletePleskLicense).ConfigureAwait(false);
            if (!legacyResponse.Success)
            {
                return ServiceResult.CreateLegacyError(legacyResponse, this.Typology, FieldNames.PleskLicense, request.ResourceId!);
            }
            else if (!legacyResponse.Result)
            {
                return ServiceResult.CreateInternalServerError();
            }
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }

    public async Task<ServiceResult<ServerCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        return await this.SearchCatalogInternal<ServerCatalog>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<Server> MapToListItem([NotNull] LegacyServerListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var serverCatalog = await this.serverCatalogRepository.GetAllAsync().ConfigureAwait(false);
        return resource.MapToListItem(userId, project, location, serverCatalog);
    }

    protected override async Task<Server> MapToDetail([NotNull] LegacyServerDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var serverName = await this.GetServerName(resource?.Model).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource!.MapToDetail(userId, project, location, serverName, profile!.IsResellerCustomer!.Value);
    }

    protected override async Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null)
    {
        var serverCatalog = await this.serverCatalogRepository.GetAllAsync().ConfigureAwait(false);
        var ret = new ServerCatalog()
        {
            TotalCount = totalCount,
            Values = items.MapToServerCatalog(serverCatalog, language).ToList()
        };
        return await Task.FromResult(ret).ConfigureAwait(false);
    }

    protected override DateTimeOffset? GetDueDate(Server resource)
    {
        return resource?.Properties?.DueDate;
    }

    private async Task<string> GetServerName(string? model)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            return string.Empty;
        }

        var serverCatalog = await this.serverCatalogRepository.GetServerCatalogAsync(model).ConfigureAwait(false);

        return serverCatalog?.ServerName ?? string.Empty;
    }

    protected override IResourceMessage GetUpdatedEvent(Server server, AutorenewFolderAction? action)
    {
        return new ServerUpdatedDeployment()
        {
            Resource = server.Map(action)
        };
    }
}
