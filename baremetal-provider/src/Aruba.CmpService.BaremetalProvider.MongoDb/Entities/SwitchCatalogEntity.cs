using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class SwitchCatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("code")]
    public string? Code { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("ports")]
    public string? Ports { get; set; }
}
