using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class SwaasCatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("code")]
    public string? Code { get; set; }

    [BsonElement("itemId")]
    public string? ItemId { get; set; }

    [BsonElement("linkedDevicesCount")]
    public int LinkedDevicesCount { get; set; }

    [BsonElement("data")]
    public IEnumerable<SwaasCatalogDataEntity> Data { get; set; } = new List<SwaasCatalogDataEntity>();
}
