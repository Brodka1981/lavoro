using System.Globalization;
using System.Text.Json;
using MongoDB.Bson;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Extensions;
internal static class BsonDocumentExtensions
{
    internal static BsonDocument? GetBsonDocumentFromJsonElement(this JsonElement? json)
     => !json.HasValue ? null : json.Value.ConvertJsonToBson();

    internal static BsonDocument ConvertJsonToBson(this JsonElement e, bool writeRootArrayAsDocument = false, bool tryParseDateTimes = false) =>
        e.ValueKind switch
        {
            JsonValueKind.Object =>
                new(e.EnumerateObject().Select(p => new BsonElement(p.Name, p.Value.ToBsonValue(tryParseDateTimes)))),
            // Newtonsoft converts arrays to documents by using the index as a key, so optionally do the same thing.
            JsonValueKind.Array when writeRootArrayAsDocument =>
                new(e.EnumerateArray().Select((v, i) => new BsonElement(i.ToString(NumberFormatInfo.InvariantInfo), v.ToBsonValue(tryParseDateTimes)))),
            _ => throw new NotSupportedException($"ToBsonDocument: {e}"),
        };

    internal static BsonValue ToBsonValue(this JsonElement e, bool tryParseDateTimes = false) =>
        e.ValueKind switch
        {
            // TODO: determine whether you want strings that look like dates & times to be serialized as DateTime, DateTimeOffset, or just strings.
            JsonValueKind.String when tryParseDateTimes && e.TryGetDateTime(out var v) => BsonValue.Create(v),
            JsonValueKind.String => BsonValue.Create(e.GetString()),
            // TODO: decide whether to convert to Int64 unconditionally, or only when the value is larger than Int32
            JsonValueKind.Number when e.TryGetInt32(out var v) => BsonValue.Create(v),
            JsonValueKind.Number when e.TryGetInt64(out var v) => BsonValue.Create(v),
            // TODO: decide whether to convert floating values to decimal by default.  Decimal has more precision but a smaller range.
            //JsonValueKind.Number when e.TryGetDecimal(out var v) => BsonValue.Create(v),
            JsonValueKind.Number when e.TryGetDouble(out var v) => BsonValue.Create(v),
            JsonValueKind.Null => BsonValue.Create(null),
            JsonValueKind.True => BsonValue.Create(true),
            JsonValueKind.False => BsonValue.Create(false),
            JsonValueKind.Array => new BsonArray(e.EnumerateArray().Select(v => v.ToBsonValue(tryParseDateTimes))),
            JsonValueKind.Object => e.ConvertJsonToBson(false, tryParseDateTimes),
            _ => throw new NotSupportedException($"ToBsonValue: {e}"),
        };
}
