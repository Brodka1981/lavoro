namespace Aruba.CmpService.BaremetalProvider.Api.Code.Middleware;

public class ArubaGatewayForwardedHeadersMiddleware
{
    /**
     * See
     * https://github.com/dotnet/aspnetcore/blob/v6.0.26/src/Middleware/HttpOverrides/src/ForwardedHeadersMiddleware.cs
     * https://github.com/dotnet/aspnetcore/blob/v6.0.26/src/Http/Http.Abstractions/src/Extensions/UsePathBaseMiddleware.cs
     */

    private const string XForwardedProtoHeaderName = "X-Forwarded-Proto";
    private const string XOriginalProtoHeaderName = "X-Original-Proto";
    private const string XForwardedHostHeaderName = "X-Forwarded-Host";
    private const string XOriginalHostHeaderName = "X-Original-Host";
    private const string XForwardedPrefixHeaderName = "X-Forwarded-Prefix";
    private const string XOriginalPrefixHeaderName = "X-Original-Prefix";

    private readonly RequestDelegate next;

    public ArubaGatewayForwardedHeadersMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var req = context.Request;

        var reqHeaders = req.Headers;

        var forwardedScheme = req.Headers[XForwardedProtoHeaderName].SingleOrDefault();
        if (forwardedScheme is not null)
        {
            // save the original
            reqHeaders[XOriginalProtoHeaderName] = req.Scheme;

            // remove the consumed values
            reqHeaders.Remove(XForwardedProtoHeaderName);

            if (forwardedScheme.Split(',').Any())
            {
                forwardedScheme = forwardedScheme.Split(',').FirstOrDefault() ?? forwardedScheme;
            }

            req.Scheme = forwardedScheme;
        }

        var forwardedHost = req.Headers[XForwardedHostHeaderName].SingleOrDefault();
        if (forwardedHost is not null)
        {
            // save the original
            reqHeaders[XOriginalHostHeaderName] = req.Host.ToString();

            // remove the consumed values
            reqHeaders.Remove(XForwardedHostHeaderName);

            req.Host = HostString.FromUriComponent(forwardedHost);
        }

        var forwardedPrefix = req.Headers[XForwardedPrefixHeaderName].SingleOrDefault();
        if (forwardedPrefix is not null)
        {
            // save the original
            reqHeaders[XOriginalPrefixHeaderName] = string.Empty;

            // remove the consumed values
            reqHeaders.Remove(XForwardedPrefixHeaderName);

            req.PathBase = req.PathBase.Add("/" + forwardedPrefix);
        }

        return next(context);
    }

}
