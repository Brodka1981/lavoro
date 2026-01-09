using System.Net;
using System.Text.Json;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Middlewares;

public class AdminLegacyHandler : BaseLegacyDelegatingHandler
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AdminLegacyHandler> logger;
    private readonly LegacyAdminCredentials legacyAdminCredentials;

    public AdminLegacyHandler(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        ILogger<AdminLegacyHandler> logger,
        IOptions<LegacyAdminCredentials> legacyAdminCredentials) : base(httpContextAccessor,logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.legacyAdminCredentials = legacyAdminCredentials.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await this.GetLegacyToken().ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(token))
        {
            //Creo un 401
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        SetDefaultHeaders(request.Headers);
        request.Headers.Add("Authorization", $"Basic {token}");

        var httpResponse = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var response = new HttpResponseMessage(httpResponse.StatusCode);
        if (httpResponse.IsSuccessStatusCode)
        {
            //Deserializzo la response 
            var stringResponseMessage = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var deserializedResponseMessage = stringResponseMessage.Deserialize<LegacyResponse>();
            Log.LogInfo(logger, "Legacy response ---> {response}", stringResponseMessage);
            HttpContent? httpContent = null;
            if (deserializedResponseMessage!.Success)
            {
                var content = deserializedResponseMessage.Body.Serialize();
                httpContent = CreateJsonContent(content);
            }
            else
            {
                var realResponseCode = CheckInternalResponseCode(deserializedResponseMessage.ResultCode!);
                response = new HttpResponseMessage(realResponseCode);
                if (realResponseCode == HttpStatusCode.BadRequest)
                {
                    var error = new ApiError()
                    {
                        Code = deserializedResponseMessage.ResultCode
                    };
                    httpContent = CreateJsonContent(error.Serialize());
                }
            }
            if (httpContent != null)
            {
                response.Content = httpContent;
            }
        }
        return response;
    }

    private async Task<string?> GetLegacyToken()
    {
        var requestId = Guid.NewGuid();
        var ret = string.Empty;

        using var httpClient = this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyAuthenticationProvider);

        SetDefaultHeaders(httpClient.DefaultRequestHeaders);
        var requestBody = new
        {
            CompanyCode = "Aruba",
            Provider = "ARUBA",
            Login = this.legacyAdminCredentials.Username,
            Password = this.legacyAdminCredentials.Password,
        };

        var result = await httpClient.CallPostAsync<SsoToken>("PostAuthorize", requestBody, this.logger).ConfigureAwait(false);

        if (result.Success && !string.IsNullOrWhiteSpace(result.Result?.Body?.SsoToken))
        {
            ret = result.Result?.Body?.SsoToken;
        }
        else
        {
            Log.LogWarning(logger, $"PostAuthorize failed:{result.Serialize()}");
            ret = null;
        }

        return ret;
    }
}