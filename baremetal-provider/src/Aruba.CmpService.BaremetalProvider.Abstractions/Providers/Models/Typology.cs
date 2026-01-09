using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Providers.Models;

/// <summary>
/// Location service model
/// </summary>
[ExcludeFromCodeCoverage(Justification = "It's a dto from providers")]

public class Typology
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? CategoryId { get; set; }

    public string? Category { get; set; }
}
