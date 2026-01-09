using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class ServerCatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("model")]
    public string? Model { get; set; }

    [BsonElement("serverName")]
    public string? ServerName { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("productCode")]
    public string? ProductCode { get; set; }

    [BsonElement("data")]
    public IEnumerable<ServerCatalogDataEntity> Data { get; set; } = new List<ServerCatalogDataEntity>();
}
