using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class MCICatalogFirewallDataEntity
{
    [BsonElement("language")]
    public string? Language { get; set; }

    [BsonElement("firewallName")]
    public string? FirewallName { get; set; }
}
