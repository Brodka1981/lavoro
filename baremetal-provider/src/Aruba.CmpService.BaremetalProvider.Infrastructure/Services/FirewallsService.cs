using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Firewalls.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class FirewallsService :
    BaseResourceService<Firewall, FirewallProperties, LegacyFirewallListItem, LegacyFirewallDetail, IFirewallsProvider>,
    IFirewallsService
{
    protected override Typologies Typology => Typologies.Firewall;
    private readonly IFirewallCatalogRepository firewallCatalogRepository;
    private readonly IFirewallsProvider firewallsProvider;

    public FirewallsService(
        ILogger<FirewallsService> logger,
        IFirewallsProvider firewallsProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        IFirewallCatalogRepository firewallCatalogRepository,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions) : base(logger, firewallsProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.firewallCatalogRepository = firewallCatalogRepository;
        this.firewallsProvider = firewallsProvider;
    }

    public async Task<ServiceResult<FirewallList>> Search(FirewallSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<FirewallList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<Firewall>> GetById(BaseGetByIdRequest<Firewall> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<ServiceResult<FirewallIpAddressList>> SearchIpAddresses(FirewallIpAddressesFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        var ret = await this.ValidateUserAndProject(nameof(SearchIpAddresses), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            return ServiceResult<FirewallIpAddressList>.FromBase(ret);
        }
        var filterRequest = request.Map();

        var legacyResponse = await this.LegacyProvider.SearchIpAddresses(filterRequest, request.ResourceId).ConfigureAwait(false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            if (legacyResponse.Err?.Code?.Equals("ERR_CLOUDDCS_REVERSEDNS_NOTFOUND", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                return new ServiceResult<FirewallIpAddressList>()
                {
                    Value = new LegacyListResponse<LegacyIpAddress>() { TotalItems = 0 }.MapToFirewallIpAddressList()
                };
            }
            else
            {
                Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(SearchIpAddresses));

                return ServiceResult<FirewallIpAddressList>.CreateInternalServerError();
            }
        }

        return new ServiceResult<FirewallIpAddressList>()
        {
            Value = legacyResponse.Result.MapToFirewallIpAddressList()
        };
    }

    public async Task<ServiceResult<FirewallVlanIdList>> GetVlanIds(FirewallVlanIdFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        var ret = await this.ValidateUserAndProject(nameof(GetVlanIds), request.UserId, request.ProjectId).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            return ServiceResult<FirewallVlanIdList>.FromBase(ret);
        }
        var filterRequest = request.Map();

        var legacyResponse = await this.firewallsProvider.GetVlanIds(filterRequest, request.ResourceId).ConfigureAwait (false);
        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetVlanIds));

            return ServiceResult<FirewallVlanIdList>.CreateInternalServerError();
        }

        return new ServiceResult<FirewallVlanIdList>()
        {
            Value = legacyResponse.Result.MapToFirewallVlanIdList()
        };
    }

    public async Task<ServiceResult> UpdateIpAddress(UpdateIpUseCaseRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.IpAddress.ThrowIfNull();

        var ret = await this.ValidateExistence(nameof(UpdateIpAddress), request.UserId, request.ResourceId, request.ProjectId, false).ConfigureAwait(false);
        if (!ret.ContinueCheckErrors)
        {
            return ret;
        }

        var data = request.Map();
        var resultResponse = await this.LegacyProvider.UpdateIpAddress(data).ConfigureAwait(false);
        if (!resultResponse.Success)
        {
            return ServiceResult.CreateLegacyError(resultResponse, Typology, FieldNames.IpAddress, request.Id.ToString(CultureInfo.InvariantCulture));
        }
        if (!resultResponse.Result)
        {
            return ServiceResult.CreateInternalServerError();
        }

        var signalREnvelope = request.CreateEnvelopeForDeploymentStatusChanged(this.Typology);
        return new ServiceResult()
        {
            Envelopes = new[] { signalREnvelope },
        };
    }
    public async Task<ServiceResult<FirewallCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        return await this.SearchCatalogInternal<FirewallCatalog>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }

    protected override async Task<Firewall> MapToListItem(LegacyFirewallListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        return resource.MapToListitem(userId, project, location);

    }

    protected override async Task<Firewall> MapToDetail(LegacyFirewallDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(userId, project, location, profile!.IsResellerCustomer!.Value);
    }
    protected override async Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null)
    {
        var firewallCatalog = await this.firewallCatalogRepository.GetAllAsync().ConfigureAwait(false);
        var ret = new FirewallCatalog()
        {
            TotalCount = totalCount,
            Values = items.MapToFirewallCatalog(firewallCatalog).ToList()
        };
        return await Task.FromResult(ret).ConfigureAwait(false);
    }
    protected override DateTimeOffset? GetDueDate(Firewall resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override IResourceMessage GetUpdatedEvent(Firewall firewall, AutorenewFolderAction? action)
    {
        return new FirewallUpdatedDeployment()
        {
            Resource = firewall.Map(action)
        };
    }
}
