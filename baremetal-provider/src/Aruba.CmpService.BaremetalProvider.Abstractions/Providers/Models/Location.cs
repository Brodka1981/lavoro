using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.Xml;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;

/// <summary>
/// Location service model
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class Location
{
    public string Id { get; set; } = string.Empty;

    public string Flag { get; set; } = string.Empty;

    public string Abbreviation { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public bool Unavailable { get; set; }

    public string CrowdInName { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public IEnumerable<DataCenter>? DataCenters { get; set; }

    public IEnumerable<TypologyDetail>? Typologies { get; set; }

    public IEnumerable<Reference>? References { get; set; }
}


/// <summary>
/// DataCenter service submodel
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]

public class DataCenter
{
    public string Id { get; set; } = string.Empty;

    public string IdLocation { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public int? NodeMaxCount { get; set; } = 0;
    public int? NodePoolMaxCount { get; set; } = 0;
    public IEnumerable<string>? AllowedService { get; set; }
}

/// <summary>
/// TypologyDetail service submodel
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class TypologyDetail
{
    public string? IdTypology { get; set; } = string.Empty;
    public int? MaxCount { get; set; } = 0;
    public int? MaxCountUser { get; set; } = 0;
    public TypologyDetailExtraInfo? ExtraInfo { get; set; }
}


/// <summary>
/// TypologyDetailExtraInfo service submodel
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]
public class TypologyDetailExtraInfo
{
    public object? ExtraInfoProperties { get; set; }
}
