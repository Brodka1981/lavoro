using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Repositories;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Deployments;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Enums;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Switches.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class SwitchesService :
    BaseResourceService<Switch, SwitchProperties, LegacySwitchListItem, LegacySwitchDetail, ISwitchesProvider>,
    ISwitchesService
{
    protected override Typologies Typology => Typologies.Switch;
    private readonly ISwitchCatalogRepository switchCatalogRepository;

    public SwitchesService(
        ILogger<SwitchesService> logger,
        ISwitchesProvider switchesProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        ISwitchCatalogRepository switchCatalogRepository,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions) :
        base(logger, switchesProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.switchCatalogRepository = switchCatalogRepository;
    }

    public async Task<ServiceResult<SwitchList>> Search(SwitchSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<SwitchList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<Switch>> GetById(BaseGetByIdRequest<Switch> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<SwitchCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        return await this.SearchCatalogInternal<SwitchCatalog>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }
    protected override async Task<Switch> MapToListItem(LegacySwitchListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        return resource.MapToListitem(userId, project, location);
    }

    protected override async Task<Switch> MapToDetail(LegacySwitchDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(userId, project, location, profile!.IsResellerCustomer!.Value);
    }

    protected override async Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null)
    {
        var switchCatalog = await this.switchCatalogRepository.GetAllAsync().ConfigureAwait(false);
        var ret = new SwitchCatalog()
        {
            TotalCount = totalCount,
            Values = items.MapToSwitchCatalog(switchCatalog).ToList()
        };
        return await Task.FromResult(ret).ConfigureAwait(false);
    }

    protected override DateTimeOffset? GetDueDate(Switch resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override IResourceMessage GetUpdatedEvent(Switch @switch, AutorenewFolderAction? action)
    {
        return new SwitchUpdatedDeployment()
        {
            Resource = @switch.Map(action)
        };
    }
}
