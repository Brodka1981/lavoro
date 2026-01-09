using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class ServerCatalogDataEntity
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("cpu")]
    public string? Cpu { get; set; }

    [BsonElement("gpu")]
    public string? Gpu { get; set; }

    [BsonElement("ram")]
    public string? Ram { get; set; }

    [BsonElement("hdd")]
    public string? Hdd { get; set; }

    [BsonElement("connectivity")]
    public string? Connectivity { get; set; }
}
