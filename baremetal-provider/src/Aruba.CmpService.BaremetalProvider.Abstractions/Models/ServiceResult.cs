using System.Diagnostics.CodeAnalysis;
using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Constants;
using Aruba.CmpService.BaremetalProvider.Abstractions.Models.Validation;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
using Aruba.MessageBus.Models;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class ServiceResult
{
    public bool NotFound { get; set; }
    public IEnumerable<IServiceResultError> Errors { get; set; } = new List<IServiceResultError>();
    public IEnumerable<Envelope> Envelopes { get; set; } = new List<Envelope>();
    public ServiceResult AddError(IServiceResultError? error)
    {
        if (error != null)
        {
            this.Errors = this.Errors.Append(error);
        }
        return this;
    }
    public ServiceResult AddEnvelopes(params Envelope[] envelopes)
    {
        foreach (var envelope in envelopes ?? Array.Empty<Envelope>())
        {
            this.Envelopes = this.Envelopes.Append(envelope);
        }
        return this;
    }

    public bool ContinueCheckErrors
    {
        get
        {
            return !this.Errors.Any() || this.Errors.OfType<BadRequestError>().Count() == this.Errors.Count();
        }
    }

    public static ServiceResult CreateNotFound(object? id)
    {
        var ret = new ServiceResult();
        ret.AddError(new NotFoundError(id));
        return ret;
    }
    public static ServiceResult CreateInternalServerError()
    {
        var ret = new ServiceResult();
        ret.AddError(new FailureError());
        return ret;
    }
    public static ServiceResult CreateForbiddenError(object? id = null)
    {
        var ret = new ServiceResult();
        ret.AddError(new ForbiddenError(id));
        return ret;
    }

    public static ServiceResult CreateBadRequestError(FieldNames fieldName, string? error)
    {
        var ret = new ServiceResult();
        ret.AddError(BadRequestError.Create(fieldName, error!));
        return ret;
    }

    public static ServiceResult CreateBadRequestError(FieldNames fieldName, LegacyLabelErrors error)
    {
        var ret = new ServiceResult();
        var badRequestError = BadRequestError.Create(fieldName, error.Message);
        badRequestError.AddLabel(error);
        ret.AddError(badRequestError);
        return ret;
    }

    public static ServiceResult CreateLegacyError(ApiCallOutput apiCall, Typologies typology, FieldNames fieldName, string? id)
    {
        apiCall.ThrowIfNull();
        if (apiCall.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(apiCall.Err?.Code))
        {
            var legacyError = LegacyLabelErrors.Create(apiCall.Err.Code, typology);
            if (legacyError != null)
            {
                return ServiceResult.CreateBadRequestError(fieldName, legacyError);
            }
            return ServiceResult.CreateInternalServerError();
        }

        if (apiCall.StatusCode == HttpStatusCode.InternalServerError)
        {
            return ServiceResult.CreateInternalServerError();
        }
        return ServiceResult.CreateNotFound(id);
    }

    public static ServiceResult FromGeneric<T>(ServiceResult<T> serviceResult)
    {
        var ret = new ServiceResult();
        ret.Errors = serviceResult.Errors;
        ret.Envelopes = serviceResult.Envelopes;
        ret.NotFound = serviceResult.NotFound;
        return ret;
    }
}

#pragma warning disable CA1000
public class ServiceResult<T> :
    ServiceResult
{
    public T? Value { get; set; }

    public static ServiceResult<T> CreateNotFound(object? id)
    {
        var ret = new ServiceResult<T>();
        ret.AddError(new NotFoundError(id));
        return ret;
    }
    public static ServiceResult<T> CreateInternalServerError()
    {
        var ret = new ServiceResult<T>();
        ret.AddError(new FailureError());
        return ret;
    }
    public static ServiceResult<T> CreateForbiddenError(object? id = null)
    {
        var ret = new ServiceResult<T>();
        ret.AddError(new ForbiddenError(id));
        return ret;
    }

    public static ServiceResult<T> CreateBadRequestError(FieldNames fieldName, string? error)
    {
        var ret = new ServiceResult<T>();
        ret.AddError(BadRequestError.Create(fieldName, error!));
        return ret;
    }

    public static ServiceResult<T> CreateBadRequestError(FieldNames fieldName, LegacyLabelErrors error)
    {
        var ret = new ServiceResult<T>();
        var badRequestError = BadRequestError.Create(fieldName, error.Message);
        badRequestError.AddLabel(error);
        ret.AddError(badRequestError);
        return ret;
    }

    public static ServiceResult<T> CreateLegacyError(ApiCallOutput apiCall, Typologies typology, FieldNames fieldName, string? id)
    {
        apiCall.ThrowIfNull();
        if (apiCall.StatusCode == HttpStatusCode.BadRequest && !string.IsNullOrWhiteSpace(apiCall.Err?.Code))
        {
            var legacyError = LegacyLabelErrors.Create(apiCall.Err.Code, typology);
            if (legacyError != null)
            {
                return ServiceResult<T>.CreateBadRequestError(fieldName, legacyError);
            }
            return ServiceResult<T>.CreateInternalServerError();
        }

        if (apiCall.StatusCode == HttpStatusCode.InternalServerError)
        {
            return ServiceResult<T>.CreateInternalServerError();
        }
        return ServiceResult<T>.CreateNotFound(id);
    }

    public static ServiceResult<T> FromBase(ServiceResult serviceResult)
    {
        var ret = new ServiceResult<T>();
        ret.Errors = serviceResult.Errors;
        ret.Envelopes = serviceResult.Envelopes;
        ret.NotFound = serviceResult.NotFound;
        return ret;
    }

    public static ServiceResult<T> Clone<C>(ServiceResult<C> serviceResult)
    {
        serviceResult.ThrowIfNull();
        var ret = new ServiceResult<T>();
        ret.Errors = serviceResult.Errors;
        ret.Envelopes = serviceResult.Envelopes;
        ret.NotFound = serviceResult.NotFound;
        return ret;
    }
}
#pragma warning restore CA1000

