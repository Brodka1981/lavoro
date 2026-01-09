using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;

// FIXME: @matteo.filippa use actual properties
public class HPCCatalogEntity : IEntity
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
    public IEnumerable<HPCCatalogServerDataEntity> ServerData { get; set; } = new List<HPCCatalogServerDataEntity>();

    [BsonElement("firewallCode")]
    public string? FirewallCode { get; set; }

    [BsonElement("firewallData")]
    public IEnumerable<HPCCatalogFirewallDataEntity> FirewallData { get; set; } = new List<HPCCatalogFirewallDataEntity>();
}
