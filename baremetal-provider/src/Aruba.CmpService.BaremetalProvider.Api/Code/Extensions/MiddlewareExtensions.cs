using Aruba.CmpService.BaremetalProvider.Api.Code.Middleware;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Extensions;

public static class MiddlewareExtensions
{

    public static IApplicationBuilder UseGatewayForwardedHeaderMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ArubaGatewayForwardedHeadersMiddleware>();
        return app;
    }
}
