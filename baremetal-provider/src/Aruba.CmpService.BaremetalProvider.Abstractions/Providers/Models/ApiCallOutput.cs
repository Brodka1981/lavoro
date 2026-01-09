using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json.Serialization;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;
using Microsoft.AspNetCore.Mvc;
using Throw;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;
/// <summary>
/// Compliant con la RFC 7807 su come deve essere gestito l'inoltro al client dei dettagli di un errore (es. BadRequest)
/// da parte di una API
/// </summary>
public class ApiError : ProblemDetails
{
    public const string GenericError = "GenericError";
    public const string ValidationError = "ValidationError";

    /// <summary>
    /// Codice di errore
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Builder method di un ApiError
    /// </summary>
    /// <param name="errocCode">codice di errore</param>
    /// <param name="errorMessage">messaggio di errore</param>
    /// <param name="type">endpoint chiamato</param>
    /// <returns></returns>
    public static ApiError New(HttpStatusCode statusCode, string errocCode, string errorMessage, string type)
    {
        return new ApiError
        {
            Status = (int)statusCode,
            Type = type,
            Title = "Api Call Error",
            Detail = errorMessage,
            Code = errocCode
        };
    }

    /// <summary>
    /// Descrizione di ApiError
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"{Code} - {Detail}";
    }

    /// <summary>
    /// Errori di validazione
    /// </summary>    
    [JsonPropertyName("errors")]
    public Collection<ProblemDetailError> Errors { get; } = new();
}

/// <summary>
/// Output di una chiamata ad un api
/// </summary>

public class ApiCallOutput : IApiCallOutput
{

    /// <summary>
    /// Indica se la chiamata è andata a buon fine
    /// </summary>
    public bool Success { get; set; }


    /// <summary>
    /// Descrittore dell'errore in caso di chiamata ko
    /// </summary>
    public ApiError? Err { get; set; }

    /// <summary>
    /// Stato della chiamata
    /// </summary>
    public HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Messaggio di errore nel caso di 500
    /// </summary>
    public string? InternalServerError { get; set; }


    public override string ToString()
    {
        if (Success)
            return "Ok";
        if (InternalServerError is not null)
            return $"KO: {InternalServerError}";
        if (Err is not null)
            return $"KO: {Err}";
        return "KO";
    }
}

/// <summary>
/// Output di una chiamata ad un api
/// </summary>
/// <typeparam name="T">tipo dell'oggetto in response</typeparam>
public class ApiCallOutput<T> : ApiCallOutput
{
    public ApiCallOutput()
    {

    }
    public ApiCallOutput(T? value)
    {
        Result = value;
        Success = true;
    }

    /// <summary>
    /// EVentuale oggetto restituito dall'api
    /// </summary>
    public T? Result { get; set; }

    public ApiCallOutput<T> Clone<C>(ApiCallOutput<C> origin)
    {
        origin.ThrowIfNull();
        return new ApiCallOutput<T>()
        {
            Err = origin.Err,
            InternalServerError = origin.InternalServerError,
            StatusCode = origin.StatusCode,
            Success = origin.Success,
        };
    }
}
public abstract record BaseError(string? ErrorMessage)
{
}
public record ProblemDetailError(string? Field, string? ErrorMessage) : BaseError(ErrorMessage)
{
}
