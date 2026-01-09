using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class SmartStorageCatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("code")]
    public string? Code { get; set; }

    [BsonElement("size")]
    public string? Size { get; set; }

    [BsonElement("snapshot")]
    public string? Snapshot { get; set; }

    [BsonElement("replica")]
    public bool Replica { get; set; }
}
