using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

public class MCICatalogEntity : IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("bundleConfigurationCode")]
    public string? BundleConfigurationCode { get; set; }

    [BsonElement("serverCode")]
    public string? ServerCode { get; set; }

    [BsonElement("serverName")]
    public string? ServerName { get; set; }

    [BsonElement("category")]
    public string? Category { get; set; }

    [BsonElement("serverData")]
    public IEnumerable<MCICatalogServerDataEntity> ServerData { get; set; } = new List<MCICatalogServerDataEntity>();

    [BsonElement("firewallCode")]
    public string? FirewallCode { get; set; }

    [BsonElement("firewallData")]
    public IEnumerable<MCICatalogFirewallDataEntity> FirewallData { get; set; } = new List<MCICatalogFirewallDataEntity>();
}
