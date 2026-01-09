using System.Text.Json.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DisableMode
{
    Manual,
    Automatic
}