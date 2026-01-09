using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.Swagger;

internal static class SwaggerExtensions
{
    internal static void AddProducesResponse(this ActionModel action, HttpStatusCode statusCode, Type? type = default)
    {
        if (type != default)
        {
            action.Filters.Add(new ProducesResponseTypeAttribute(type, (int)statusCode));
        }
        else
        {
            action.Filters.Add(new ProducesResponseTypeAttribute((int)statusCode));
        }
    }
}
