using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Serialization;

public class EnumJsonConverter<T> : JsonConverter<T?>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var type = typeToConvert;
        if (Nullable.GetUnderlyingType(typeToConvert) != null)
        {
            type = Nullable.GetUnderlyingType(typeToConvert);
        }
        string stringValue = null;
        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt64(out var intValue))
            {
                stringValue = Enum.GetName(type, intValue);
            }
        }
        else if (reader.TokenType == JsonTokenType.String)
        {
            stringValue = reader.GetString();
        }
        if (!string.IsNullOrEmpty(stringValue) && Enum.TryParse(type, stringValue, true, out var enumValue))
        {
            return (T)enumValue;
        }
        return default(T);
    }

    public override void Write(Utf8JsonWriter writer, T? enumValue, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    private Type GetNumberBaseType(Type type)
    {
        return Enum.GetUnderlyingType(type);
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class EnumJsonConverterAttribute : JsonConverterAttribute
{
    public override JsonConverter? CreateConverter(Type typeToConvert)
    {
        var converter = typeof(EnumJsonConverter<>).MakeGenericType(typeToConvert);
        return Activator.CreateInstance(converter) as JsonConverter;
    }
}
