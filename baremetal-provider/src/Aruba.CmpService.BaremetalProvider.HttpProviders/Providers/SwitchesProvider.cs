using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Switches;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class SwitchesProvider :
    LegacyProvider<LegacySwitchListItem, LegacySwitchDetail>,
    ISwitchesProvider
{
    public SwitchesProvider(IHttpClientFactory httpClientFactory,
        ILogger<SwitchesProvider> logger) :
        base(httpClientFactory, logger)
    {
    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacySwitchListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();

        var fieldMapping = GetDefaultSortFieldMapping();
        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcsswitches/getlist");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacySwitchDetail>> GetById(long id)
    {
        return await this.GetById($"/dc-services/api/clouddcsswitches/getDetail?id={id}").ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsswitches/postsetcustomname", resourceRename).ConfigureAwait(false);
    }
    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal($"/dc-services/api/clouddcsswitches/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal($"/dc-services/api/clouddcsswitches/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        return await base.GetCatalog($"/dc-services/api/clouddcsswitches/getavailablemodels").ConfigureAwait(false);
    }
}
