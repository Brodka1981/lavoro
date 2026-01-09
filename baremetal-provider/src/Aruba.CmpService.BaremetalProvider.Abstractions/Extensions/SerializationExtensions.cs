using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Extensions;
public static class SerializationExtensions
{
    public static T? Deserialize<T>(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return default(T);
        }

        return JsonSerializer.Deserialize<T>(value, new JsonSerializerOptions()
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            PropertyNameCaseInsensitive = true
        });
    }
    public static string Serialize<T>(this T value)
    {
        return JsonSerializer.Serialize(value, new JsonSerializerOptions()
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            PropertyNameCaseInsensitive = true
        });
    }

    public static string? ToYaml<T>(this T value)
    {
        if (value != null)
        {
            var serializer = new SerializerBuilder().Build();
            return serializer.Serialize(value);
        }
        return null;
    }
}
