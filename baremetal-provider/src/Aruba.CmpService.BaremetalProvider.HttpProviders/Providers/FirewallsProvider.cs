using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Aruba.CmpService.ResourceProvider.Common.ResourceQuery.Filtering;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class FirewallsProvider :
    LegacyProvider<LegacyFirewallListItem, LegacyFirewallDetail>,
    IFirewallsProvider
{
    public FirewallsProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<FirewallsProvider> logger) : base(httpClientFactory, logger)
    {
    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacyFirewallListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();

        var fieldMapping = GetDefaultSortFieldMapping();
        fieldMapping.Add("ConfigurationMode", "configurationmode");

        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcsfirewalls/getlist");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacyFirewallDetail>> GetById(long id)
    {
        return await this.GetById($"/dc-services/api/clouddcsfirewalls/getDetail?id={id}").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsfirewalls/postsetcustomname", resourceRename).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyListResponse<LegacyIpAddress>>> SearchIpAddresses(LegacySearchFilters filterRequest, long firewallId)
    {
        filterRequest.ThrowIfNull();
        filterRequest.Query.Filters.Add(FilterDefinition.Create("id", "eq", firewallId));
        return await base.SearchIpAddresses("/dc-services/api/clouddcsfirewalls/getReverseDNS", filterRequest).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> UpdateIpAddress(UpdateIpAddress updateIp)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsfirewalls/postReverseDNS", updateIp).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        return await base.GetCatalog($"/dc-services/api/clouddcsfirewalls/getavailablemodels").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal($"/dc-services/api/clouddcsfirewalls/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal($"/dc-services/api/clouddcsfirewalls/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyListResponse<LegacyVlanId>>> GetVlanIds(LegacySearchFilters filterRequest, long firewallId)
    {
        var fieldMapping = GetDefaultSortFieldMapping();
        fieldMapping.Add("VLANID", "vlanId");
        var url = filterRequest.SetSort(fieldMapping).ToQueryString($"/dc-services/api/clouddcsfirewalls/getVlanIds?Id={firewallId}");

        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<LegacyListResponse<LegacyVlanId>>(url).ConfigureAwait(false);
    }
}
