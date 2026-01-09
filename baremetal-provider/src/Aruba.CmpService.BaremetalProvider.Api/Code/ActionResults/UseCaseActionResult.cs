using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.MessageBus;
using Aruba.MessageBus.UseCases;
using Aruba.MessageBus.UseCases.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;

public class UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, TType> :
    ActionResult
    where TMessageBusRequest : Request
    where TMessageBusResponse : MessageBusResponse<TType>
    where TType : class
{
    private readonly IMessageBus messageBus;
    private readonly TMessageBusRequest request;
    private readonly Func<TType, object> mapperFunc;
    private readonly HttpStatusCode? statusCode;

    public UseCaseActionResult(IMessageBus messageBus, TMessageBusRequest request, Func<TType, object> mapperFunc, HttpStatusCode? statusCode = null)
    {
        this.messageBus = messageBus;
        this.request = request;
        this.mapperFunc = mapperFunc;
        this.statusCode = statusCode;
    }

    public override async Task ExecuteResultAsync(ActionContext context)
    {
        context.ThrowIfNull();
        var result = await messageBus.ExecuteAsync<TMessageBusRequest, TMessageBusResponse>(request, CancellationToken.None)
            .ConfigureAwait(false);
        ActionResult actionResult = new OkResult();
        if (result is UseCaseActivatorResult<TMessageBusResponse>.Success { Response: MessageBusResponse<TType> response })
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

        await actionResult.ExecuteResultAsync(context).ConfigureAwait(false);
    }

    private ActionResult GetActionResult(ActionContext context, TType? value)
    {
        var mappedValue = mapperFunc != null && value != null ? mapperFunc(value) : value;
        var currentStatusCode = (int?)this.statusCode;
        if (!currentStatusCode.HasValue)
        {
            var statusCodeAttributes = context.ActionDescriptor.FilterDescriptors.Select(s => s.Filter).OfType<ProducesResponseTypeAttribute>().ToList();
            var successAttribute = statusCodeAttributes.Find(f => f.StatusCode >= 200 && f.StatusCode <= 299);
            if (successAttribute != null)
            {
                currentStatusCode = successAttribute.StatusCode;
            }
        }
        if (currentStatusCode.HasValue)
        {
            if (value == null)
            {
                return new NoContentResult();
            }
            return new ObjectResult(mappedValue)
            {
                StatusCode = currentStatusCode.Value,
            };
        }
        var method = context.ActionDescriptor.ActionConstraints.OfType<HttpMethodActionConstraint>().First().HttpMethods.First().ToUpperInvariant();
        return method switch
        {
            "GET" => new OkObjectResult(mappedValue),
            "POST" => mappedValue != null ? new OkObjectResult(mappedValue) { StatusCode = StatusCodes.Status201Created } : new NoContentResult(),
            "PUT" => mappedValue != null ? new OkObjectResult(mappedValue) { StatusCode = StatusCodes.Status202Accepted } : new NoContentResult(),
            "DELETE" => new NoContentResult(),
            _ => new StatusCodeResult(500)
        };
    }
}
