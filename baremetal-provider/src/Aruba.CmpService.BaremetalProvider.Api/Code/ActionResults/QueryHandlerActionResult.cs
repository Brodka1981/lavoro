using System.Collections;
using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Microsoft.AspNetCore.Mvc;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;
public class QueryHandlerActionResult<TRequest, TResponse> :
    ActionResult
{
    private readonly IQueryService queryService;
    private readonly TRequest request;
    private readonly Func<TResponse, object> mapperFunc;
    private readonly HttpStatusCode? statusCode;

    public QueryHandlerActionResult(IQueryService queryService, TRequest request, Func<TResponse, object> mapperFunc, HttpStatusCode? statusCode = null)
    {
        this.queryService = queryService;
        this.request = request;
        this.mapperFunc = mapperFunc;
        this.statusCode = statusCode;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        var result = await queryService.ExecuteHandler<TRequest, TResponse>(request)
            .ConfigureAwait(false);

        var actionResult = GetActionResult(result);
        await actionResult.ExecuteResultAsync(context).ConfigureAwait(false);
    }

    private static bool IsCollection()
    {
        return typeof(TResponse).GetInterface(nameof(IEnumerable)) != null;
    }

    private ActionResult GetActionResult(TResponse value)
    {
        if (value == null)
        {
            if (IsCollection())
            {
                return new NoContentResult();
            }
            else
            {
                return new NotFoundResult();
            }
        }
        else
        {
            if (statusCode.HasValue)
            {

                return new ObjectResult(mapperFunc != null ? mapperFunc(value) : value)
                {
                    StatusCode = (int)statusCode.Value
                };
            }
            else
            {
                return new OkObjectResult(mapperFunc != null ? mapperFunc(value) : value);
            }
        }
    }
}
