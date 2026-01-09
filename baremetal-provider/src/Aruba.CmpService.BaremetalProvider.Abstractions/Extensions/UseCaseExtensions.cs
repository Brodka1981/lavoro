using System.Collections.Concurrent;
using System.Linq.Expressions;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.MessageBus.Models;
using Aruba.MessageBus.UseCases;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
public static class UseCaseExtensions
{
    private static ConcurrentDictionary<Type, Func<object>> _contructors = new ConcurrentDictionary<Type, Func<object>>();

    public static HandlerResult<TResponse> ExecutedResult<TRequest, TResponse, TValue>(this UseCase<TRequest, TResponse> useCase, ServiceResult<TValue> serviceResult)
        where TRequest : class
        where TResponse : MessageBusResponse<TValue>
    {
        useCase.ThrowIfNull();
        serviceResult.ThrowIfNull();
        var notFoundError = serviceResult.Errors.OfType<NotFoundError>().FirstOrDefault();
        if (notFoundError != null)
        {
            return useCase.ExecutedNotFound(notFoundError.Id);
        }
        var forbiddenError = serviceResult.Errors.OfType<ForbiddenError>().FirstOrDefault();
        if (forbiddenError != null)
        {
            return useCase.ExecutedForbidden(forbiddenError.Id);
        }
        var badRequestErrors = serviceResult.Errors.OfType<BadRequestError>().ToArray();
        if (badRequestErrors.Any())
        {
            return useCase.ExecutedValidation(serviceResult.Envelopes.ToArray(), badRequestErrors);
        }
        var failureErrors = serviceResult.Errors.OfType<FailureError>().ToArray();
        if (failureErrors.Any())
        {
            return useCase.ExecutedFailure(new Exception(failureErrors.First().ErrorMessage), serviceResult.Envelopes.ToArray());
        }
        return useCase.ExecutedSuccess(serviceResult.Value, serviceResult.Envelopes.ToArray());
    }

    public static HandlerResult<TResponse> ExecutedResult<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, ServiceResult serviceResult)
        where TRequest : class
        where TResponse : EmptyMessageBusResponse
    {
        useCase.ThrowIfNull();
        serviceResult.ThrowIfNull();
        var notFoundError = serviceResult.Errors.OfType<NotFoundError>().FirstOrDefault();
        if (notFoundError != null)
        {
            return useCase.ExecutedNotFound(notFoundError.Id);
        }
        var forbiddenError = serviceResult.Errors.OfType<ForbiddenError>().FirstOrDefault();
        if (forbiddenError != null)
        {
            return useCase.ExecutedForbidden(forbiddenError.Id);
        }
        var badRequestErrors = serviceResult.Errors.OfType<BadRequestError>().ToArray();
        if (badRequestErrors.Any())
        {
            return useCase.ExecutedValidation(serviceResult.Envelopes.ToArray(), badRequestErrors);
        }
        var failureErrors = serviceResult.Errors.OfType<FailureError>().ToArray();
        if (failureErrors.Any())
        {
            return useCase.ExecutedFailure(new Exception(failureErrors.First().ErrorMessage), serviceResult.Envelopes.ToArray());
        }
        return useCase.ExecutedSuccess(serviceResult.Envelopes.ToArray());
    }

    public static HandlerResult<TResponse> ExecutedSuccess<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, params Envelope[] envelopes)
        where TRequest : class
        where TResponse : EmptyMessageBusResponse
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        return useCase.Executed(ret, envelopes);
    }

    public static HandlerResult<TResponse> ExecutedSuccess<TRequest, TResponse, TValue>(this UseCase<TRequest, TResponse> useCase, TValue? value, params Envelope[] envelopes)
        where TRequest : class
        where TResponse : MessageBusResponse<TValue>
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        ret.Value = value;
        return useCase.Executed(ret, envelopes);
    }

    public static HandlerResult<TResponse> ExecutedFailure<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, Exception exception, Envelope[]? envelopes = null)
        where TRequest : class
        where TResponse : MessageBusResponse
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        ret.Exception = exception;
        ret.Type = MessageBusResponseTypes.Failure;
        return envelopes != null ? useCase.Executed(ret, envelopes) : useCase.Executed(ret);
    }

    public static HandlerResult<TResponse> ExecutedValidation<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, Envelope[]? envelopes = null, params BadRequestError[] errors)
        where TRequest : class
        where TResponse : MessageBusResponse
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        ret.Errors = errors;
        ret.Type = MessageBusResponseTypes.Validation;
        return envelopes != null ? useCase.Executed(ret, envelopes) : useCase.Executed(ret);
    }

    public static HandlerResult<TResponse> ExecutedForbidden<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, object? id)
        where TRequest : class
        where TResponse : MessageBusResponse
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        ret.Id = id;
        ret.Type = MessageBusResponseTypes.Forbidden;
        return useCase.Executed(ret);
    }

    public static HandlerResult<TResponse> ExecutedNotFound<TRequest, TResponse>(this UseCase<TRequest, TResponse> useCase, object? id)
        where TRequest : class
        where TResponse : MessageBusResponse
    {
        useCase.ThrowIfNull();

        var ret = CreateInstance<TResponse>();
        ret.Id = id;
        ret.Type = MessageBusResponseTypes.NotFound;
        return useCase.Executed(ret);

    }

    private static T CreateInstance<T>() where T : class
    {
        if (!_contructors.ContainsKey(typeof(T)))
        {
            _contructors.TryAdd(typeof(T), CreateConstructor<T>());
        }

        return (_contructors[typeof(T)]() as T)!;


    }

    private static Func<object> CreateConstructor<T>()
    {
        var constructor = typeof(T).GetConstructor(Array.Empty<Type>())!;
        var creatorExpression = Expression.Lambda<Func<T>>(
            Expression.New(constructor, Array.Empty<Expression>()));
        return (creatorExpression.Compile() as Func<object>)!;
    }
}
