using System.Net.Http;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.SmartStorages;
using Aruba.CmpService.BaremetalProvider.Abstractions.TechnoRail.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public abstract class LegacyProvider<TResourceListItem, TResourceDetail> :
    ILegacyProvider<TResourceListItem, TResourceDetail>
    where TResourceListItem : LegacyResourceListItem
    where TResourceDetail : LegacyResourceDetail
{
    protected IHttpClientFactory HttpClientFactory { get; }
    protected ILogger<LegacyProvider<TResourceListItem, TResourceDetail>> Logger { get; }

    protected LegacyProvider(IHttpClientFactory httpClientFactory, ILogger<LegacyProvider<TResourceListItem, TResourceDetail>> logger)
    {
        HttpClientFactory = httpClientFactory;
        this.Logger = logger;
    }

    protected async Task<ApiCallOutput<LegacyListResponse<TResourceListItem>>> Search(string url)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<LegacyListResponse<TResourceListItem>>(url).ConfigureAwait(false);
    }

    protected async Task<ApiCallOutput<TResourceDetail>> GetById(string url)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<TResourceDetail>(url).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> Rename(string url, ResourceRename resourceRename)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>(url, resourceRename).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> UpsertAutomaticRenewInternal(string url, ResourceUpsertAutomaticRenew resourceUpsertAutomaticRenew)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>(url, resourceUpsertAutomaticRenew).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<bool>> DeleteAutomaticRenewInternal(string url, ResourceDeleteAutomaticRenew resourceDeleteAutomaticRenew)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallPostAsync<bool>(url, resourceDeleteAutomaticRenew).ConfigureAwait(false);

    }

    protected async Task<ApiCallOutput<LegacyListResponse<LegacyIpAddress>>> SearchIpAddresses(string baseUrl, LegacySearchFilters filterRequest)
    {
        filterRequest.ThrowIfNull();
        var url = filterRequest.ToIpAddressQueryString(baseUrl);
        using var httpClient = this.CreateHttpClient();
        var ret = await httpClient.CallGetAsync<LegacyListResponse<LegacyIpAddress>>(url).ConfigureAwait(false);
        if (ret?.Result is null)
        {
            Log.LogWarning(Logger, "SearchIpAddresses result is null {response}", ret.Serialize());
        }

        return ret;
    }

    public async Task<ApiCallOutput<bool>> UpdateIpAddress(string url, UpdateIpAddress updateIp)
    {
        using var httpClient = this.CreateHttpClient();
        var ret = await httpClient.CallPostAsync<bool>(url, updateIp).ConfigureAwait(false);
        if (ret?.Result is null)
        {
            Log.LogWarning(Logger, "UpdateIpAddress result is null {response}", ret.Serialize());
        }

        return ret;
    }

    protected HttpClient CreateHttpClient()
    {
        return this.HttpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyProvider);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog(string url)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<IEnumerable<LegacyCatalog>>(url).ConfigureAwait(false);
    }

    protected static IDictionary<string, string> GetDefaultSortFieldMapping()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"NAME","name" },
            {"STATE","status" },
            {"DUEDATE","expirationDate" },
            {"ACTIVATIONDATE","activationDate" },
            {"MODEL","model" },
        };
    }

    public async Task<ApiCallOutput<LegacySmartFolders>> GetFolders(string url)
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<LegacySmartFolders>(url).ConfigureAwait(false);
    }

    #region interface members
    public abstract Task<ApiCallOutput<LegacyListResponse<TResourceListItem>>> Search(LegacySearchFilters filterRequest);
    public abstract Task<ApiCallOutput<TResourceDetail>> GetById(long id);
    public abstract Task<ApiCallOutput<bool>> Rename(ResourceRename resourceRename);
    public abstract Task<ApiCallOutput<IEnumerable<LegacyCatalog>>> GetCatalog();
    public abstract Task<ApiCallOutput<bool>> UpsertAutomaticRenew(ResourceUpsertAutomaticRenew upsertAutomaticRenew);
    public abstract Task<ApiCallOutput<bool>> DeleteAutomaticRenew(ResourceDeleteAutomaticRenew deleteAutomaticRenew);
    #endregion
}
