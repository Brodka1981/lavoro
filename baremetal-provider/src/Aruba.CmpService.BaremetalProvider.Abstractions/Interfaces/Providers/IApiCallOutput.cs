using System.Net;
using Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Providers;

/// <summary>
/// Interface of ApiCallOuput
/// </summary>
public interface IApiCallOutput
{
    /// <summary>
    /// Error
    /// </summary>
    ApiError? Err { get; set; }

    /// <summary>
    /// InternalServerError
    /// </summary>
    string? InternalServerError { get; set; }

    /// <summary>
    /// HttpStatusCode
    /// </summary>
    HttpStatusCode StatusCode { get; set; }

    /// <summary>
    /// Success
    /// </summary>
    bool Success { get; set; }
}
