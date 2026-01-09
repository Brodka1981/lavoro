using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.MCI;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
internal class MCIsProvider : LegacyProvider<LegacyMCIListItem, LegacyMCIDetail>,
                              IMCIsProvider
{
    public MCIsProvider(
        IHttpClientFactory httpClientFactory,
        ILogger<MCIsProvider> logger
        ) : base(httpClientFactory, logger)
    {

    }

    public override async Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew)
    {
        return await base.DeleteAutomaticRenewInternal($"/dc-services/api/clouddcsbundles/postsetautorenew", deleteAutomaticRenew).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacyMCIDetail>> GetById(long id)
    {
        return await this.GetById($"/dc-services/api/clouddcsbundles/getDetail?Id={id}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyMCIDetail>> GetContentById(long id, LegacySearchFilters filterRequest)
    {
        var fieldMapping = GetDefaultSortFieldMapping();
        fieldMapping.Add("MCISERVICENAME", "mciServiceName");
        fieldMapping.Add("MCISERVICETYPE", "mciServiceType");
        var url = filterRequest.SetSort(fieldMapping).ToQueryString($"/dc-services/api/clouddcsbundles/getDetail?Id={id}");

        return await this.GetById(url).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<List<MCIConfiguration>>> GetMCICatalog()
    {
        using var httpClient = this.CreateHttpClient();
        var catalog = await httpClient.CallGetAsync<List<MCIConfiguration>>($"/dc-services/api/clouddcsbundles/getavailablemodels?bundleType=MCI").ConfigureAwait(false);
        return catalog;
    }

    public override async Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>($"/dc-services/api/clouddcsbundles/postsetcustomname", resourceRename).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<LegacyListResponse<LegacyMCIListItem>>> Search(LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();

        var fieldMapping = GetDefaultSortFieldMapping();
        fieldMapping.Add("NUMSERVICES", "numServices");
        fieldMapping.Add("CREATIONDATE", "creationDate");

        var url = filterRequest.SetSort(fieldMapping).ToQueryString("/dc-services/api/clouddcsbundles/getlist?bundleType=MCI");
        return await base.Search(url).ConfigureAwait(false);
    }

    public override async Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew)
    {
        return await base.UpsertAutomaticRenewInternal($"/dc-services/api/clouddcsbundles/postsetautorenew", upsertAutomaticRenew).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyMCIDetail>> GetByIdWithPrices(long id, bool calculatePrices)
    {
        return await this.GetById($"/dc-services/api/clouddcsbundles/getDetail?Id={id}&calculateprices={calculatePrices.ToString()}").ConfigureAwait(false);
    }

    public override Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog()
    {
        throw new NotImplementedException();
    }
}
