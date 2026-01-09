using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Services;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Aruba.CmpService.BaremetalProvider.HttpProviders.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Middlewares;

internal class LegacyHandler :
    BaseLegacyDelegatingHandler
{
    private readonly ITokenProvider tokenProvider;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<LegacyHandler> logger;
    private readonly IProfileProvider profileProvider;

    public LegacyHandler(
        ITokenProvider tokenProvider,
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        ILogger<LegacyHandler> logger,
        IProfileProvider profileProvider) : base(httpContextAccessor, logger)
    {
        this.tokenProvider = tokenProvider;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        this.profileProvider = profileProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.GetUserId();
        string? token = null;
        if (!string.IsNullOrWhiteSpace(userId))
        {
            token = await this.GetLegacyToken(userId).ConfigureAwait(false);
        }
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

    private async Task<string?> GetLegacyToken(string userId)
    {
        var requestId = Guid.NewGuid();
        var ret = await tokenProvider.GetToken(userId).ConfigureAwait(false);
        if (ret == null)
        {  
            Log.LogInfo(logger, "Token not found in collection");
            var tokenLegacyClaim = httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == "legacy_token")?.Value;
            if (!string.IsNullOrWhiteSpace(tokenLegacyClaim))
            {
                Log.LogInfo(logger, "Decode Token from httpContext");
                var decodeLegacyClaim = Encoding.UTF8.GetString(Convert.FromBase64String(tokenLegacyClaim));
                ret = decodeLegacyClaim.Deserialize<JsonObject>()["Token"].GetValue<string?>();
            }
            if (!string.IsNullOrWhiteSpace(ret))
            {
                using var httpClient = this.httpClientFactory.CreateServiceClient(BaremetalHttpClientNames.LegacyAuthenticationProvider);

                SetDefaultHeaders(httpClient.DefaultRequestHeaders);
                var requestBody = new
                {
                    CompanyCode = "Aruba",
                    Provider = await this.GetProviderAsync(userId).ConfigureAwait(false),
                    //Login = "10769323@aruba.it",
                    //Password = "123456789Ab!"
                    Login = userId,
                    Password = ret
                };

                var result = await httpClient.CallPostAsync<SsoToken>("PostAuthorize", requestBody, this.logger).ConfigureAwait(false);

                if (result.Success && !string.IsNullOrWhiteSpace(result.Result?.Body?.SsoToken))
                {
                    ret = result.Result?.Body?.SsoToken;
                }
                else
                {
                    Log.LogWarning(logger, $"risposta PostAuthorize:{result.Serialize()}");
                    ret = null;
                }

            }

            if (!string.IsNullOrWhiteSpace(ret))
            {
                //Salvo il token
                await this.tokenProvider.SetToken(ret, userId).ConfigureAwait(false);
            }
        }
        return ret;
    }

    private async Task<string> GetProviderAsync(string userId)
    {
        var profileResponse = await this.profileProvider.GetUser(userId).ConfigureAwait(false);

        if (profileResponse.Success
            && profileResponse.Result != null
            && (profileResponse.Result.IsResellerCustomer ?? false))
        {
            Log.LogInfo(logger, $"received IsResellerCustomer:{profileResponse.Result.IsResellerCustomer}--> use CLOUD_RSCST legacy provider");
            return "CLOUD_RSCST";
        }

        Log.LogInfo(logger, $"received IsResellerCustomer:{profileResponse?.Result?.IsResellerCustomer} --> use CLOUD legacy provider");
        return "CLOUD";
    }
}