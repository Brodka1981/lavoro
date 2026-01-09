using System.Diagnostics.CodeAnalysis;
using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Mapping;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Firewalls;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Servers;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Switches;
using Aruba.CmpService.BaremetalProvider.Abstractions.QueryHandlers;
using Aruba.CmpService.ResourceProvider.Common;
using Aruba.MessageBus;
using Aruba.MessageBus.UseCases;
using Aruba.MessageBus.UseCases.Requests;
using Microsoft.AspNetCore.Mvc;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.SwaaSes;

namespace Aruba.CmpService.BaremetalProvider.Api.Code.ActionResults;

public static class ControllerExtensions
{
    #region Query Handlers
    public static QueryHandlerActionResult<TRequest, TResponse> QueryHandler<TRequest, TResponse>(this ControllerBase controllerBase, TRequest request)
    {
        var queryService = controllerBase.HttpContext.RequestServices.GetRequiredService<IQueryService>();
        return new QueryHandlerActionResult<TRequest, TResponse>(queryService, request, o => o);
    }
    public static QueryHandlerActionResult<TRequest, TResponse> QueryHandler<TRequest, TResponse>(this ControllerBase controllerBase, TRequest request, HttpStatusCode statusCode)
    {
        var queryService = controllerBase.HttpContext.RequestServices.GetRequiredService<IQueryService>();
        return new QueryHandlerActionResult<TRequest, TResponse>(queryService, request, o => o, statusCode);
    }
    public static QueryHandlerActionResult<TRequest, TResponse> QueryHandler<TRequest, TResponse>(this ControllerBase controllerBase, TRequest request, Func<TResponse, object> mapperFunc)
    {
        var queryService = controllerBase.HttpContext.RequestServices.GetRequiredService<IQueryService>();
        return new QueryHandlerActionResult<TRequest, TResponse>(queryService, request, mapperFunc);
    }
    public static QueryHandlerActionResult<TRequest, TResponse> QueryHandler<TRequest, TResponse>(this ControllerBase controllerBase, TRequest request, Func<TResponse, object> mapperFunc, HttpStatusCode statusCode)
    {
        var queryService = controllerBase.HttpContext.RequestServices.GetRequiredService<IQueryService>();
        return new QueryHandlerActionResult<TRequest, TResponse>(queryService, request, mapperFunc, statusCode);
    }
    #endregion

    #region UseCase With Results
    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Server> UseCaseServer<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
        where TMessageBusRequest : Request
        where TMessageBusResponse : MessageBusResponse<Server>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Server>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Switch> UseCaseSwitch<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
       where TMessageBusRequest : Request
       where TMessageBusResponse : MessageBusResponse<Switch>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Switch>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Swaas> UseCaseSwaases<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
       where TMessageBusRequest : Request
       where TMessageBusResponse : MessageBusResponse<Swaas>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Swaas>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, VirtualSwitch> UseCaseVirtualSwitches<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
       where TMessageBusRequest : Request
       where TMessageBusResponse : MessageBusResponse<VirtualSwitch>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, VirtualSwitch>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, VirtualSwitchLink> UseCaseVirtualSwitchLinks<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
       where TMessageBusRequest : Request
       where TMessageBusResponse : MessageBusResponse<VirtualSwitchLink>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, VirtualSwitchLink>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Firewall> UseCaseFirewall<TMessageBusRequest, TMessageBusResponse>([NotNull] this ControllerBase controllerBase, TMessageBusRequest request)
       where TMessageBusRequest : Request
       where TMessageBusResponse : MessageBusResponse<Firewall>
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        var source = controllerBase.HttpContext.Request.Headers[RequestHeaders.Source];
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, Firewall>(messageBusService, request, v => v.MapToResponse());
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, TType> UseCase<TMessageBusRequest, TMessageBusResponse, TType>(this ControllerBase controllerBase, TMessageBusRequest request)
        where TMessageBusRequest : Request
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, TType>(messageBusService, request, o => o);
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, TType> UseCase<TMessageBusRequest, TMessageBusResponse, TType>(this ControllerBase controllerBase, TMessageBusRequest request, Func<TType, object> mapperFunc)
        where TMessageBusRequest : Request
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, TType>(messageBusService, request, mapperFunc);
    }

    public static UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, object> UseCase<TMessageBusRequest, TMessageBusResponse>(this ControllerBase controllerBase, TMessageBusRequest request)
        where TMessageBusRequest : Request
        where TMessageBusResponse : EmptyMessageBusResponse
    {
        var messageBusService = controllerBase.HttpContext.RequestServices.GetRequiredService<IMessageBus>();
        return new UseCaseActionResult<TMessageBusRequest, TMessageBusResponse, object>(messageBusService, request, o => o, HttpStatusCode.NoContent);
    }
    #endregion

    #region Bus
    public static BusResponseActionResult<TMessageBusResponse, TType> Bus<TMessageBusResponse, TType>(this ControllerBase controllerBase, UseCaseActivatorResult<TMessageBusResponse> response)
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        return new BusResponseActionResult<TMessageBusResponse, TType>(response, o => o);
    }
    public static BusResponseActionResult<TMessageBusResponse, TType> Bus<TMessageBusResponse, TType>(this ControllerBase controllerBase, UseCaseActivatorResult<TMessageBusResponse> response, HttpStatusCode statusCode)
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        return new BusResponseActionResult<TMessageBusResponse, TType>(response, o => o, statusCode);
    }
    public static BusResponseActionResult<TMessageBusResponse, TType> Bus<TMessageBusResponse, TType>(this ControllerBase controllerBase, UseCaseActivatorResult<TMessageBusResponse> response, Func<TType, object> mapperFunc)
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        return new BusResponseActionResult<TMessageBusResponse, TType>(response, mapperFunc);
    }
    public static BusResponseActionResult<TMessageBusResponse, TType> Bus<TMessageBusResponse, TType>(this ControllerBase controllerBase, UseCaseActivatorResult<TMessageBusResponse> response, Func<TType, object> mapperFunc, HttpStatusCode? statusCode)
        where TMessageBusResponse : MessageBusResponse<TType>
        where TType : class
    {
        return new BusResponseActionResult<TMessageBusResponse, TType>(response, mapperFunc, statusCode);
    }
    #endregion
}
