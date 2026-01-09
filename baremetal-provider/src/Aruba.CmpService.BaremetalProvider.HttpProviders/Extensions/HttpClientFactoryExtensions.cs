namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;

public static class HttpClientFactoryExtensions
{
    public static HttpClient CreateServiceClient(this IHttpClientFactory httpClientFactory, BaremetalHttpClientNames name)
    {
        return httpClientFactory.CreateClient($"{name.Value}_service");
    }
    public static HttpClient CreateUserClient(this IHttpClientFactory httpClientFactory, BaremetalHttpClientNames name)
    {
        return httpClientFactory.CreateClient($"{name.Value}_user");
    }
}
