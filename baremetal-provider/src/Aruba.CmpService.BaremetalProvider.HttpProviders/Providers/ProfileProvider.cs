using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Profile;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;
public class ProfileProvider :
    IProfileProvider
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<ProfileProvider> logger;


    public ProfileProvider(IHttpClientFactory httpClientFactory, ILogger<ProfileProvider> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }
    public async Task<ApiCallOutput<UserProfile>> GetUser(string userId)
    {
        using var httpClient = CreateClient();
        var output = await httpClient
            .CallGetAsync<UserProfile>($"users/{userId}?api-version=1.0")
            .ConfigureAwait(false);

        return output;
    }

    private HttpClient CreateClient()
        => httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.ProfileProvider);


}
