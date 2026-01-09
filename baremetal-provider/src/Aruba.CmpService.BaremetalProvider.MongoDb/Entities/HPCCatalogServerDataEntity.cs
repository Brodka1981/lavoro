using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class HPCCatalogServerDataEntity
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("cpu")]
    public string? Cpu { get; set; }

    [BsonElement("hdd")]
    public string? Hdd { get; set; }

    [BsonElement("ram")]
    public string? Ram { get; set; }

    [BsonElement("nodeNumber")]
    public string? NodeNumber { get; set; }

    [BsonElement("hardwareName")]
    public string? HardwareName { get; set; }
}
