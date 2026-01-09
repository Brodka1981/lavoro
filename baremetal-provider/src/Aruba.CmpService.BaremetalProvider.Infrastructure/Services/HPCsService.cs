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
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.HPCs;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.HPC;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers.Payments.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.Common.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.UseCases.HPCs.Requests;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Sorting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Infrastructure.Services;

// FIXME: @matteo.filippa use proper generic types
public class HPCsService :
    BaseResourceService<HPC, HPCProperties, LegacyHPCListItem, LegacyHPCDetail, IHPCsProvider>, IHPCsService
{
    private readonly IHPCCatalogRepository hpcCatalogRepository;
    private readonly IPaymentsService paymentsService;
    private readonly IHPCsProvider hpcProvider;
    protected override Typologies Typology => Typologies.HPC;

    public HPCsService(
        ILogger<HPCsService> logger,
        IHPCsProvider hpcsProvider,
        IProjectProvider projectProvider,
        ILocationMapRepository locationMapRepository,
        IHPCCatalogRepository hpcCatalogRepository,
        IOptions<RenewFrequencyOptions> renewFrequencyOptions,
        IOptions<BaremetalOptions> baremetalOptions,
        IPaymentsProvider paymentsProvider,
        ICatalogueProvider catalogueProvider,
        IProfileProvider profileProvider,
        IOptions<EnableUpdatedEventOptions> enableUpdatedEventOptions,
        IPaymentsService paymentsService) : base(logger, hpcsProvider, projectProvider, locationMapRepository, renewFrequencyOptions, baremetalOptions, catalogueProvider, paymentsProvider, profileProvider, enableUpdatedEventOptions)
    {
        this.hpcCatalogRepository = hpcCatalogRepository;
        this.hpcProvider = hpcsProvider;
        this.paymentsService = paymentsService;
    }

    protected override DateTimeOffset? GetDueDate(HPC resource)
    {
        return resource?.Properties?.DueDate;
    }

    protected override async Task<HPC> MapToDetail(LegacyHPCDetail resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        var profile = await this.GetProfile(userId).ConfigureAwait(false);
        return resource.MapToDetail(location, project, profile!.IsResellerCustomer!.Value);
    }

    protected override async Task<HPC> MapToListItem(LegacyHPCListItem resource, string userId, Abstractions.Providers.Models.Projects.Project project)
    {
        var location = await this.GetLocation(resource.ServerFarmCode).ConfigureAwait(false);
        return resource.MapToListitem(userId, project, location);
    }

    /// <summary>
    /// NOTE: This method implementation is temporary to comply with CONSIP level 3 by the end of January 2026.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<ServiceResult<HPC>> Create(HPCCreateUseCaseRequest request, CancellationToken cancellationToken)
    {
        // 1. Make call to OneArch API to create HPC order

        // 2. Pay order using wallet (or do nothing if pre-paid by reseller)
        var payOrderRequest = new PayOrderRequest
        {
            OrderId = "", // TODO: @martellata-hpc set proper OrderId
            ServiceType = LegacyServiceType.Server // TODO: @martellata-hpc set proper ServiceType
        };
        var payOrderResponse = await paymentsService
            .PayOrderWithWalletAsync(request!.UserId!, payOrderRequest, cancellationToken)
            .ConfigureAwait(false);

        if (payOrderResponse.Errors.Any())
        {
            // FIXME: @martellata-hpc Log errors
            return ServiceResult<HPC>.CreateInternalServerError();
        }

        var res = new ServiceResult<HPC>()
        {
            Value = new HPC()
            {
                // TODO: @martellata-hpc Insert proper HPC properties
            }
        };
        return res;
    }

    public async Task<ServiceResult<HPCList>> Search(HPCSearchFilterRequest request, CancellationToken cancellationToken)
    {
        return await base.SearchInternal<HPCList>(request, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<HPC>> GetById(BaseGetByIdRequest<HPC> request, CancellationToken cancellationToken)
    {
        return await this.GetByIdInternal(request.ResourceId, request.UserId, request.ProjectId, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ServiceResult<HPC>> GetByIdWithPrices(HPCByIdRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        var legacyResponse = await this.hpcProvider.GetByIdWithPrices(long.Parse(request.ResourceId), request.CalculatePrices).ConfigureAwait(false);

        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetByIdWithPrices));
            return ServiceResult<HPC>.CreateInternalServerError();
        }

        var legacyItem = legacyResponse.Result;

        var location = await this.GetLocation(legacyItem.ServerFarmCode).ConfigureAwait(false);

        var projectResponse = await this.ProjectProvider.GetProjectAsync(request.UserId!, request.ProjectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(GetContentById), request.ProjectId);
            return ServiceResult<HPC>.CreateNotFound(request.ProjectId);
        }

        var profile = await this.GetProfile(request.UserId!).ConfigureAwait(false);

        return new ServiceResult<HPC>()
        {
            Value = legacyResponse.Result.MapToDetail(location, projectResponse.Result, profile!.IsResellerCustomer!.Value)
        };
    }

    public async Task<ServiceResult<HPC>> GetContentById(HPCContentByIdRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();
        request.ResourceId.ThrowIfNull();

        var searchFilter = new LegacySearchFilters()
        {
            Query = request.Query
        };

        var legacyResponse = await this.hpcProvider.GetContentById(long.Parse(request.ResourceId), searchFilter).ConfigureAwait(false);

        if (!legacyResponse.Success || legacyResponse.Result == null)
        {
            Log.LogWarning(Logger, "{MethodName} > resource retrieve errors", nameof(GetContentById));
            return ServiceResult<HPC>.CreateInternalServerError();
        }

        var legacyItem = legacyResponse.Result;

        var location = await this.GetLocation(legacyItem.ServerFarmCode).ConfigureAwait(false);

        var projectResponse = await this.ProjectProvider.GetProjectAsync(request.UserId!, request.ProjectId!).ConfigureAwait(false);
        if (!projectResponse.Success || projectResponse.Result == null || !(projectResponse.Result.Properties?.Default ?? false))
        {
            Log.LogWarning(Logger, "{MethodName} > invalid projectId {projectId}", nameof(GetContentById), request.ProjectId);
            return ServiceResult<HPC>.CreateNotFound(request.ProjectId);
        }

        return new ServiceResult<HPC>()
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

    protected override IResourceMessage GetUpdatedEvent(HPC resource, AutorenewFolderAction? action)
    {
        return new HPCUpdatedDeployment()
        {
            Resource = resource.Map(action)
        };
    }

    public async Task<ServiceResult<HPCCatalog>> SearchCatalog(CatalogFilterRequest request, CancellationToken cancellationToken)
    {
        request.ThrowIfNull();

        var configurationResult = await hpcProvider.GetHPCCatalog().ConfigureAwait(false);

        if (!configurationResult.Success)
        {
            switch (configurationResult.StatusCode)
            {
                case System.Net.HttpStatusCode.Unauthorized:
                    return ServiceResult<HPCCatalog>.CreateForbiddenError();
                default:
                    return ServiceResult<HPCCatalog>.CreateInternalServerError();
            }
        }

        var configurationList = configurationResult.Result;
        var values = new List<HPCCatalogItem>();

        var ret = new HPCCatalog()
        {
            TotalCount = values.Count,
            Values = values
        };

        var result = new ServiceResult<HPCCatalog>() { Value = ret };

        if (configurationList == null || configurationList.Count <= 0)
        {
            return await Task.FromResult(result).ConfigureAwait(false);
        }

        var mciCatalog = await this.hpcCatalogRepository.GetAllAsync().ConfigureAwait(false);

        //gestisco la ricerca
        var textFilter = request.Query.Filters.Where(f => f.FieldName.Equals("fulltextsearch", StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

        if (textFilter != null)
        {
            var textToSearch = JsonConvert.DeserializeObject<string>(textFilter.Argument.RawValue);
            switch (textFilter.OperatorSymbol.ToUpperInvariant())
            {
                case "EQ":
                    mciCatalog = mciCatalog.Where(opt => opt.Data.FirstOrDefault()!.HardwareName!.Contains(textToSearch!, StringComparison.OrdinalIgnoreCase)).ToList();
                    break;
                default:
                    break;
            }
        }

        values = (List<HPCCatalogItem>)mciCatalog.MapToHPCCatalog(request.Language, configurationList);

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

        ret = new HPCCatalog()
        {
            TotalCount = values.Count,
            Values = values
        };

        result = new ServiceResult<HPCCatalog>() { Value = ret };

        return await Task.FromResult(result).ConfigureAwait(false);
    }

}
