using Amazon.Auth.AccessControlPolicy;
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
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.MCIs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.MCIs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;
public class MCIsService :
    BaseResourceService<MCI, MCIProperties, LegacyMCIListItem, LegacyMCIDetail, IMCIsProvider>, IMCIsService
{
    private readonly IMCICatalogRepository mciCatalogRepository;
    private readonly IMCIsProvider mciProvider;
    protected override Typologies Typology => Typologies.MCI;

    public MCIsService(
        ILogger<MCIsService> logger,
        IMCIsProvider mcisProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IMCICatalogRepository mciCatalogRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,        
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions) : base(logger, mcisProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.mciCatalogRepository = mciCatalogRepository;
        this.mciProvider = mcisProvider;
    }
    protected override DateTimeOffset? GetDueDate(MCI resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override async Task<MCI> MapToDetail(LegacyMCIDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(location, project, profile!.IsResellerCustomer!.Value);
    }

    protected override async Task<MCI> MapToListItem(LegacyMCIListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        return resource.MapToListitem(userId, project, location);
    }

    public async Task<ServiceResult<MCIList>> Search(MCISearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<MCIList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<MCI>> GetById(BaseGetByIdRequest<MCI> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<MCI>> GetByIdWithPrices(MCIByIdRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        var legacyResponse = await this.mciProvider.GetByIdWithPrices(long.Parse(request.ResourceId), request.CalculatePrices).ConfigureAwait(false);

        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetByIdWithPrices));
            return ServiceResult<MCI>.CreateInternalServerError();
        }

        var legacyItem = legacyResponse.Result;

        var location = await this.GetLocation(legacyItem.ServerFarmCode).ConfigureAwait(false);

        var projectResponse = await this.ProjectProvider.GetProjectAsync(request.UserId!, request.ProjectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(GetContentById), request.ProjectId);
            return ServiceResult<MCI>.CreateNotFound(request.ProjectId);
        }

        var profile = await this.GetProfile(request.UserId!).ConfigureAwait(false);

        return new ServiceResult<MCI>()
        {
            Value = legacyResponse.Result.MapToDetail(location, projectResponse.Result, profile!.IsResellerCustomer!.Value)
        };
    }

    public async Task<ServiceResult<MCI>> GetContentById(MCIContentByIdRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        var searchFilter = new LegacySearchFilters()
        {
            Query = request.Query
        };

        var legacyResponse = await this.mciProvider.GetContentById(long.Parse(request.ResourceId), searchFilter).ConfigureAwait(false);

        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetContentById));
            return ServiceResult<MCI>.CreateInternalServerError();
        }

        var legacyItem = legacyResponse.Result;

        var location = await this.GetLocation(legacyItem.ServerFarmCode).ConfigureAwait(false);

        var projectResponse = await this.ProjectProvider.GetProjectAsync(request.UserId!, request.ProjectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(GetContentById), request.ProjectId);
            return ServiceResult<MCI>.CreateNotFound(request.ProjectId);
        }

        return new ServiceResult<MCI>()
        {
            Value = legacyResponse.Result.MapToDetail(location, projectResponse.Result)
        };

    }

    public async Task<ServiceResult> Rename(RenameUseCaseRequest request, CancellationToken cancellationToken)
    {
        var result = await base.RenameInternal(request, cancellationToken).ConfigureAwait(false);
        return result;
    }

    public async Task<ServiceResult> SetAutomaticRenew(SetAutomaticRenewUseCaseRequest request, CancellationToken cancellationToken)
    {
        return await this.SetAutomaticRenewInternal(request, cancellationToken).ConfigureAwait(false);
    }
    
    protected override Task<ICatalog> MapCatalog(IEnumerable<LegacyCatalogItem> items, long totalCount, string? language = null)
    {
        throw new NotImplementedException();
    }

    protected override IResourceMessage GetUpdatedEvent(MCI resource, AutorenewFolderAction? action)
    {
        return new MCIUpdatedDeployment()
        {
            Resource = resource.Map(action)
        };
    }

    public async Task<ServiceResult<MCICatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        var configurationResult = await mciProvider.GetMCICatalog().ConfigureAwait(false);

        if (!configurationResult.Success)
        {
            switch(configurationResult.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    return ServiceResult<MCICatalog>.CreateForbiddenError();
                default:
                    return ServiceResult<MCICatalog>.CreateInternalServerError();
            }
        }

        var configurationList = configurationResult.Result;
        var values = new List<MCICatalogItem>();

        var ret = new MCICatalog()
        {
            TotalCount = values.Count,
            Values = values
        };

        var result = new ServiceResult<MCICatalog>() { Value = ret };

        if (configurationList == null || configurationList.Count <= 0) 
        {
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        var mciCatalog = await this.mciCatalogRepository.GetAllAsync().ConfigureAwait(false);

        //gestisco la ricerca
        var textFilter = request.Query.Filters.Where(f => f.FieldName.Equals("fulltextsearch", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        if (textFilter != null)
        {
            var textToSearch = JsonConvert.DeserializeObject<string>(textFilter.Argument.RawValue);
            switch (textFilter.OperatorSymbol.ToUpperInvariant())
            {
                case "EQ":
                    mciCatalog = mciCatalog.Where(opt => opt.Data.FirstOrDefault()!.HardwareName!.Contains(textToSearch!,StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                default:
                    break;
            }
        }

        values = (List<MCICatalogItem>)mciCatalog.MapToMCICatalog(request.Language, configurationList);

        if (values.Count > 0)
        {
            //Ora ordino per nome o prezzo
            string? sortField = request.Query.Sorts.FirstOrDefault()?.FieldName?.ToUpperInvariant();
            bool sortDescending = request.Query.Sorts.FirstOrDefault()?.Direction == SortDirection.Descending;
            switch (sortField)
            {
                case "PRICE":
                    values = values.SortBy(opt => opt.Price, !sortDescending).ToList();
                    break;
                case "SERVERNAME":
                    values = values.SortBy(opt => opt.ServerName, !sortDescending).ToList();
                    break;
                default:
                    values = values.OrderBy(opt => opt.Price).ToList();
                    break;
            }
        }

        ret = new MCICatalog()
        {
            TotalCount = values.Count,
            Values = values
        };

        result = new ServiceResult<MCICatalog>() { Value = ret };

        return await Task.FromResult(result).ConfigureAwait(false);
    }
}
