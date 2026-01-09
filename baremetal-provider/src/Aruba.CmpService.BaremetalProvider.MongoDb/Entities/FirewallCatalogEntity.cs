using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class FirewallCatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("code")]
    public string? Code { get; set; }

    [BsonElement("mode")]
    public string? Mode { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }
}
