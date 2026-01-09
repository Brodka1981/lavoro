using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
public class DataProtectionKeyEntity
{
    /// <summary>
    /// The entity identifier of the <see cref="DataProtectionKeyEntity"/>.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; }

    /// <summary>
    /// The friendly name of the <see cref="DataProtectionKeyEntity"/>.
    /// </summary>

    [BsonElement("friendlyName")]
    public string? FriendlyName { get; set; }

    /// <summary>
    /// The XML representation of the <see cref="DataProtectionKeyEntity"/>.
    /// </summary>
    [BsonElement("xml")]
    public string? Xml { get; set; }
}
