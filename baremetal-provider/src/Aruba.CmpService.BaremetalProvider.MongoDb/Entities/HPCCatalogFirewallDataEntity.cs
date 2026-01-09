using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class HPCCatalogFirewallDataEntity
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("firewallName")]
    public string? FirewallName { get; set; }
}
