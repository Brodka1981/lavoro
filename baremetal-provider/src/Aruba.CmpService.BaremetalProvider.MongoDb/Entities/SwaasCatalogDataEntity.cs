using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class SwaasCatalogDataEntity
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("model")]
    public string? Model { get; set; }
}
