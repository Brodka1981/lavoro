using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SsoToken
{
    [JsonPropertyName("Body")]
    public SSoBody? Body { get; set; }

    [JsonPropertyName("Success")]
    public bool Success { get; set; }

    [JsonPropertyName("ResultCode")]
    public string? ResultCode { get; set; }

    [JsonPropertyName("ResultMessage")]
    public string? ResultMessage { get; set; }

    [JsonPropertyName("ExceptionMessage")]
    public string? ExceptionMessage { get; set; }
}


[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class SSoBody
{
    [JsonPropertyName("SsoToken")]
    public string? SsoToken { get; set; }
}
