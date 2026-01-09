using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.MessageBus.UseCases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;

public class BusResponseActionResult<TMessageBusResponse, TType> :
    ActionResult
    where TMessageBusResponse : MessageBusResponse<TType>
    where TType : class
{
    private readonly UseCaseActivatorResult<TMessageBusResponse> result;
    private readonly Func<TType, object> mapperFunc;
    private readonly HttpStatusCode? statusCode;



    public BusResponseActionResult(UseCaseActivatorResult<TMessageBusResponse> result, Func<TType, object> mapperFunc, HttpStatusCode? statusCode = null)
    {
        this.result = result;
        this.mapperFunc = mapperFunc;
        this.statusCode = statusCode;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        ActionResult actionResult = new OkResult();
        if (result is UseCaseActivatorResult<TMessageBusResponse>.Success { Response: MessageBusResponse<TType> response } success)
        {
            actionResult = response.Type switch
            {
                MessageBusResponseTypes.Success => GetActionResult(context, response.Value),
                MessageBusResponseTypes.NotFound => new StatusCodeResult(StatusCodes.Status404NotFound),
                MessageBusResponseTypes.Validation => new BadRequestObjectResult(context.CreateProblemDetails(response.Errors)),
                MessageBusResponseTypes.Failure => new StatusCodeResult(StatusCodes.Status500InternalServerError),
                MessageBusResponseTypes.Forbidden => new StatusCodeResult(StatusCodes.Status403Forbidden),
                _ => new BadRequestResult()
            };
        }
        else if (result is UseCaseActivatorResult<TMessageBusResponse>.Failure { Exception.SourceException: var ex })
        {
            throw ex;
        }

        await actionResult.ExecuteResultAsync(context);
    }

    private ActionResult GetActionResult(ActionContext context, TType value)
    {
        if (statusCode.HasValue)
        {
            if (value == null)
            {
                return new NoContentResult();
            }
            return new ObjectResult(mapperFunc != null ? mapperFunc(value) : value)
            {
                StatusCode = (int)statusCode.Value
            };
        }
        var method = context.ActionDescriptor.ActionConstraints.OfType<HttpMethodActionConstraint>().First().HttpMethods.First().ToUpperInvariant();
        return method switch
        {
            "GET" => new OkObjectResult(value),
            "POST" => new OkObjectResult(value) { StatusCode = StatusCodes.Status201Created },
            "PUT" => new OkObjectResult(value) { StatusCode = StatusCodes.Status202Accepted },
            "DELETE" => new NoContentResult(),
            _ => new StatusCodeResult(500)
        };
    }
}
