using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;

public class CatalogueProvider : ICatalogueProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<CatalogueProvider> logger;

    public CatalogueProvider(IHttpClientFactory httpClientFactory, ILogger<CatalogueProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<ApiCallOutput<Location>> GetLocationByValue(string value)
    {
        return await this.GetLocation($"api/v1/service/locations/value/{value}").ConfigureAwait(false);

    }

    private async Task<ApiCallOutput<Location>> GetLocation(string url)
    {
        Log.LogDebug(logger, "Retrieve location from {url}", url);

        using var client = this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.CatalogueProvider);

        var ret = await client.CallGetAsync<Location>(url).ConfigureAwait(false);

        Log.LogDebug(logger, "Location response ---> {ret}", ret.Serialize());

        if (ret.Success && ret.Result is not null)
        {
            return ret;
        }
        else
        {
            Log.LogWarning(logger, "GetLocaiton response: {response}", ret.Serialize());

            return new ApiCallOutput<Location>
            {
                StatusCode = HttpStatusCode.NotFound,
                Err = ApiError.New(HttpStatusCode.NotFound, "404", "LOCATIONS.NOT.FOUND", "GetLocation")
            };
        }
    }
}
