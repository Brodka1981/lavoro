using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
public class LocationMapEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("value")]
    public string? Value { get; set; }

    [BsonElement("legacyValue")]
    public string? LegacyValue { get; set; }
}
