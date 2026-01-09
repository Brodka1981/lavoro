using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Aruba.CmpService.BaremetalProvider.Abstractions.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Aruba.CmpService.BaremetalProvider.HttpProviders.Middlewares;
public abstract class BaseLegacyDelegatingHandler : DelegatingHandler
{
    protected IHttpContextAccessor httpContextAccessor { get; }

    protected BaseLegacyDelegatingHandler(IHttpContextAccessor httpContextAccessor, ILogger<BaseLegacyDelegatingHandler> logger)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.logger = logger;
    }

    private static string[] _notFoundErrors = new [] {
        "NOT_FOUND",
        "ERR_CLOUDDCS_DELETEPLESKLICENSE_LICENSE_NOT_FOUND",
        "ERR_CLOUDDCS_REVERSEDNS_NOT_FOUND",
        "ERR_CLOUDDCS_SERVICE_NOTFOUND",
        "ERR_CLOUDDCS_SWAAS_NOTSWAAS",
        "ERR_CLOUDDCS_SWAAS_VIRTUALNETWORKNOTEXISTS"
    };

    private static string[] _internalServerErrors = new[] {
        "ERR_SYS_MSGBROKER_CLIENT_RESPONSE_TIMEOUT_ABORTED",
        "ERR_PAYMENT_PUTAUTOMATICRENEWAL_FAILED",
        "ERR_CLOUDDCS_GENERIC",
        "ERR_CLOUDDCS_SERVER_REBOOT_CLIENTIP_MISSING" ,
        "ERR_GETFRAUDRISKASSESSMENT_FAILED",
        "ERR_PAYMENT_GETDEVICES_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_SETPASSWORD_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_CREATESMARTFOLDER_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_ENABLESERVICE_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_CREATESNAPSHOT_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_RESTORESNAPSHOT_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_COMMUNICATION",
        "ERR_CLOUDDCS_SMARTSTORAGE_ENABLESNAPSHOTTASK_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_CREATESNAPSHOTTASK_FAILED",
        "ERR_CLOUDDCS_SMARTSTORAGE_DELETESNAPSHOTTASK_FAILED",
        "ERR_CLOUDDCS_USERNAME_MISSING",
        "ERR_SYS_MSGBROKER_CLIENT",
        "ERR_CLOUDDCS_SWAAS_POSTVIRTUALNETWORK",
        "ERR_CLOUDDCS_SWAAS_GETVIRTUALNETWORKS",
        "ERR_CLOUDDCS_SWAAS_UPDATEVIRTUALNETWORK_FRIENDLYNAME",
        "ERR_CLOUDDCS_SWAAS_DELETEVIRTUALNETWORK",
        "ERR_CLOUDDCS_SWAAS_GETALLRESOURCES",
        "ERR_CLOUDDCS_SWAAS_POSTVIRTUALNETWORKRESOURCE",
        "ERR_CLOUDDCS_SWAAS_VIRTUALNETWORKLOCKED",
        "ERR_CLOUDDCS_SWAAS_PODFAILS",
        "ERR_CLOUDDCS_SWAAS_DELETEVIRTUALNETWORKRESOURCE",
        "ERR_CLOUDDCS_SWAAS_GETSERVICESWITHREGION"
    };
    private readonly ILogger<BaseLegacyDelegatingHandler> logger;

    protected static HttpStatusCode CheckInternalResponseCode(string responseCode)
    {
        if (_notFoundErrors.FirstOrDefault(f => f.Equals(responseCode, StringComparison.OrdinalIgnoreCase)) != null)
        {
            return HttpStatusCode.NotFound;
        }
        if (_internalServerErrors.FirstOrDefault(f => f.Equals(responseCode, StringComparison.OrdinalIgnoreCase)) != null)
        {
            return HttpStatusCode.InternalServerError;
        }
        if (string.IsNullOrWhiteSpace(responseCode))
        {
            return HttpStatusCode.InternalServerError;
        }
        return HttpStatusCode.BadRequest;
    }

    protected static HttpContent CreateJsonContent(string content)
    {
        return new StringContent(content, Encoding.UTF8, "application/json");
    }
    protected void SetDefaultHeaders(HttpRequestHeaders headers)
    {
        string clientIp = this.httpContextAccessor?.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0] ?? "127.0.0.1";
        Log.LogWarning(logger, $"ClientIp: {clientIp}");
        headers.Add("X-ClientIP", clientIp);
        headers.Add("X-AppName", "cmp-baremetal-provider");
        headers.Add("Host", "api.aruba.it");
        headers.Add("Accept", "application/json");
        headers.Add("X-IsAsync", "False");
        headers.Add("X-SessionID", "");
    }

}
