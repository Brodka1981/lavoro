using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class InternalLegacyProvider : IInternalLegacyProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<InternalLegacyProvider> logger;

    public InternalLegacyProvider(IHttpClientFactory httpClientFactory, ILogger<InternalLegacyProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    /// <inheritdoc cref="IInternalLegacyProvider.UpsertAutomaticRenewAsync(LegacyAutoRenew)"/>
    public async Task<ApiCallOutput<bool>> UpsertAutomaticRenewAsync(LegacyAutoRenew body)
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallPostAsync<bool>("dc-services/api/CloudDcsServices/PostSetAutoRenew", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderServices(List<LegacyResourceFilter> services, bool getPrices)
    {
        var body = new
        {
            GetPrices = getPrices,
            Services = services,
        };

        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallPostAsync<IEnumerable<LegacyResource>>("dc-services/api/CloudDcsServices/PostGetLookupList", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetLegacyResources()
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallGetAsync<IEnumerable<LegacyResource>>("dc-services/api/CloudDcsServices/GetLookupList?getPrices=false").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyAutorechargeData>> GetAutorechargeAsync()
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallGetAsync<LegacyAutorechargeData>("dc-services/api/clouddcscloudcomputing/getuserautorechargeinfo").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyRegion>>> GetRegions()
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallGetAsync<IEnumerable<LegacyRegion>>("dc-services/api/CloudDcsSwaas/GetRegions").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyServiceWithRegion>>> GetServicesWithRegions()
    {
        using var httpClient = this.CreateHttpClient();
        return await httpClient.CallGetAsync<IEnumerable<LegacyServiceWithRegion>>("dc-services/api/clouddcsswaas/getserviceswithregions").ConfigureAwait(false);
    }

    private HttpClient CreateHttpClient()
    {
        return this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyProvider);
    }
}


