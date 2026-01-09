using System.Diagnostics.CodeAnalysis;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[ExcludeFromCodeCoverage(Justification = "It's a model class without logic")]
public class Version
{
    /// <summary>
    /// Version data
    /// </summary>
    public DataVersion? Data { get; set; }

    /// <summary>
    /// Model
    /// </summary>
    public string? Model { get; set; }
}
