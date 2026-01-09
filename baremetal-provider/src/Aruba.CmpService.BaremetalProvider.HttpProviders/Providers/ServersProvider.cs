using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class ServersProvider :
    LegacyProvider<LegacyServerListItem, LegacyServerDetail>,
    IServersProvider
{

    public ServersProvider(IHttpClientFactory httpClientFactory,
        ILogger<ServersProvider> logger) :
        base(httpClientFactory, logger)
    {
    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacyServerListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();
        var fieldMapping = GetDefaultSortFieldMapping();

        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcsservers/getlist");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacyServerDetail>> GetById(long id)
    {
        return await this.GetById($"/dc-services/api/clouddcsservers/getDetail?id={id}").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsservers/postsetcustomname", resourceRename, Logger).ConfigureAwait(false);
    }
    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal($"/dc-services/api/clouddcsservers/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal($"/dc-services/api/clouddcsservers/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> Restart(long id)
    {
        var data = new
        {
            Id = id,
            CheckOnly = false
        };
        using var httpClient = this.HttpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyProvider);
        return await httpClient.CallPostAsync<bool>("/dc-services/api/clouddcsservers/postreboot", data, Logger).ConfigureAwait(false);
    }
    public async Task<ApiCallOutput<bool>> UpdateIpAddress(UpdateIpAddress updateIp)
    {
        return await base.UpdateIpAddress($"/dc-services/api/clouddcsservers/postReverseDNS", updateIp).ConfigureAwait(false);
    }
    public async Task<ApiCallOutput<LegacyListResponse<LegacyIpAddress>>> SearchIpAddresses(LegacySearchFilters filterRequest, long serverId)
    {
        filterRequest.ThrowIfNull();
        filterRequest.Query.Filters.Add(FilterDefinition.Create("id", "eq", serverId));
        return await base.SearchIpAddresses("/dc-services/api/clouddcsservers/getReverseDNS", filterRequest).ConfigureAwait(false);
    }
    public override async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        return await base.GetCatalog($"/dc-services/api/clouddcsservers/getavailablemodels").ConfigureAwait(false);
    }
    public async Task<ApiCallOutput<bool>> DeletePleskLicense(DeletePleskLicense deletePleLicense)
    {
        using var httpClient = this.CreateHttpClient();
        Log.LogInfo(Logger, "DeletePleskLicense body {deletePleskLicense}", deletePleLicense.Serialize());
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsservers/postdeleteplesklicense", deletePleLicense, Logger).ConfigureAwait(false);
    }
}
