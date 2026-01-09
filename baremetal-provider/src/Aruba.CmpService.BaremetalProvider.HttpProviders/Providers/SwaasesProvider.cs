using System.Globalization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Swaases;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class SwaasesProvider :
    LegacyProvider<LegacySwaasListItem, LegacySwaasDetail>,
    ISwaasesProvider
{
    public SwaasesProvider(IHttpClientFactory httpClientFactory,
        ILogger<LegacyProvider<LegacySwaasListItem, LegacySwaasDetail>> logger) :
        base(httpClientFactory, logger)
    {

    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacySwaasListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();
        var fieldMapping = GetDefaultSortFieldMapping();

        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcsswaas/getlist");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacySwaasDetail>> GetById(long id)
    {
        return await this.GetById($"/dc-services/api/clouddcsswaas/getDetail?Id={id}").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsswaas/postsetcustomname", resourceRename, Logger).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        return await base.GetCatalog($"/dc-services/api/clouddcsswaas/getavailablemodels").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal("/dc-services/api/clouddcsswaas/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal("/dc-services/api/clouddcsswaas/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<List<LegacyVirtualSwitch>>> GetVirtualSwitches(string swaasId)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<List<LegacyVirtualSwitch>>($"/dc-services/api/clouddcsswaas/getvirtualnetworks?id={swaasId}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyVirtualSwitch>> AddVirtualSwitch(AddLegacyVirtualSwitch virtualSwitch)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<LegacyVirtualSwitch>("/dc-services/api/clouddcsswaas/postcreatevirtualnetwork", virtualSwitch).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyVirtualSwitch>> EditVirtualSwitch(EditLegacyVirtualSwitch virtualSwitch)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<LegacyVirtualSwitch>("/dc-services/api/clouddcsswaas/postsetvirtualnetworkfriendlyname", virtualSwitch).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput> DeleteVirtualSwitch(DeleteLegacyVirtualSwitch virtualSwitch)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync("/dc-services/api/clouddcsswaas/postdeletevirtualnetwork", virtualSwitch).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<List<LegacyVirtualSwitchLink>>> GetVirtualSwitchLinks(string swaasId)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<List<LegacyVirtualSwitchLink>>($"/dc-services/api/clouddcsswaas/getvirtualnetworksresources?id={swaasId}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput> AddVirtualSwitchLink(string swaasId, string virtualSwitchId, long serviceId, LegacyServiceType serviceType)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = long.Parse(swaasId, CultureInfo.InvariantCulture),
            VirtualNetworkId = virtualSwitchId,
            ServiceToConnect = new
            {
                Id = serviceId,
                ServiceType = (int)serviceType
            },
        };
        return await httpClient.CallPostAsync($"/dc-services/api/clouddcsswaas/postcreatevirtualnetworkresource", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput> DeleteVirtualSwitchLink(string swaasId, string virtualSwitchId, string virtualSwitchLinkId)
    {
        using var httpClient = this.CreateHttpClient();
        var body = new
        {
            Id = swaasId,
            VirtualNetworkId = virtualSwitchId,
            ResourceId = virtualSwitchLinkId
        };
        return await httpClient.CallPostAsync($"/dc-services/api/clouddcsswaas/postdeletevirtualnetworkresource", body).ConfigureAwait(false);
    }
}
