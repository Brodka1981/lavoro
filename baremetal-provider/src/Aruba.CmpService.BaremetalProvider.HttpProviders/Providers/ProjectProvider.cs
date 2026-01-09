using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models.Projects;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Aruba.CmpService.ResourceProvider.Common.Dtos.Response;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Providers;

public class ProjectProvider :
    IProjectProvider
{
    private readonly IHttpClientFactory httpClientFactory;

    public ProjectProvider(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public async Task<ApiCallOutput<Project?>> GetDefaultProjectAsync(string userId)
    {
        using var httpClient = this.httpClientFactory.CreateUserClient(BaremetalHttpClientNames.ResourceManagerProvider);

        var result = await httpClient.CallGetAsync<ListResponseDto<Project>>($"/projects?api-version=1.0").ConfigureAwait(false);

        if (result.Success && result.Result?.Values != null)
        {
            var projectDetails = result.Result.Values?.Find(f => (f.Properties?.Default ?? false) && (f.Metadata.CreatedBy!.Equals(userId, StringComparison.OrdinalIgnoreCase)));
            return new ApiCallOutput<Project?>(projectDetails);

        }
        else
        {
            return new ApiCallOutput<Project?>().Clone(result);
        }
    }

    public async Task<ApiCallOutput<Project?>> GetProjectAsync(string userId, string id)
    {
        using var httpClient = this.httpClientFactory.CreateUserClient(BaremetalHttpClientNames.ResourceManagerProvider);

        var result = await httpClient.CallGetAsync<Project>($"/projects/{WebUtility.UrlEncode(id)}?api-version=1.0").ConfigureAwait(false);

        if (result.Success && result.Result != null && result.Result.Metadata.CreatedBy!.Equals(userId, StringComparison.OrdinalIgnoreCase))
        {
            return new ApiCallOutput<Project?>(result.Result);

        }
        else
        {
            return new ApiCallOutput<Project?>().Clone(result);
        }
    }
}
