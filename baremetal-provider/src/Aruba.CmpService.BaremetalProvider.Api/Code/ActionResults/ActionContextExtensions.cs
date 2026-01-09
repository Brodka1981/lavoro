using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.CmpService.ResourceProvider.Common;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;

public static class ActionContextExtensions
{
    public static ProblemDetails CreateProblemDetails(this ActionContext context, IEnumerable<IServiceResultError> errors)
    {
        return context.HttpContext.CreateProblemDetails(errors);
    }

    public static ProblemDetails CreateProblemDetails(this HttpContext context, IEnumerable<IServiceResultError> errors)
    {
        var ret = new ProblemDetails
        {
            Instance = null,
            Status = 400,
            Title = "One or more validation errors occurred.",
        };

        var caller = context.Request.Headers[RequestHeaders.Source];
        if (string.Equals(caller, RequestSource.Bff.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            ret.Extensions.Add("showErrorNextToRelativeField", true);
            ret.Extensions.Add("errors", errors);
        }
        else
        {
            var validationErrors = errors.OfType<BadRequestError>().Select(s => new ProblemDetailError(s.FieldName, s.ErrorMessage));
            ret.Extensions.Add("errors", validationErrors);
        }
        return ret;
    }
}
