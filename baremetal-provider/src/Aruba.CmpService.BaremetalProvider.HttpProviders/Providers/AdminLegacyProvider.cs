using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Legacy.Internal;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;

public class AdminLegacyProvider : IAdminLegacyProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AdminLegacyProvider> logger;

    public AdminLegacyProvider(IHttpClientFactory httpClientFactory, ILogger<AdminLegacyProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderServices(List<LegacyResourceFilter> services, string userId)
    {
        using var httpClient = this.CreateHttpClient();

        var body = new
        {
            CloudUsername = userId,
            Services = services,
            GetPrices = false
        };

        return await httpClient.CallPostAsync<IEnumerable<LegacyResource>>($"dc-services/api/CloudDcsServices/PostGetLookupListadmin", body).ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<IEnumerable<LegacyResource>>> GetFolderLinkableServices(string userId)
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallGetAsync<IEnumerable<LegacyResource>>($"dc-services/api/CloudDcsServices/GetLookupListAdmin?getPrices=false&cloudUsername={userId}").ConfigureAwait(false);
    }

    public async Task<ApiCallOutput<LegacyAutorechargeData>> GetAutorechargeAsync(string userId)
    {
        using var httpClient = this.CreateHttpClient();

        return await httpClient.CallGetAsync<LegacyAutorechargeData>($"dc-services/api/clouddcscloudcomputing/getuserautorechargeinfoadmin?cloudUsername={userId}").ConfigureAwait(false);
    }

    private HttpClient CreateHttpClient()
    {
        return this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.AdminLegacyProvider);
    }
}


