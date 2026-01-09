using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
public class TokenEntity :
    IEntity
{
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonId]
    public string? Id { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("token")]
    public string? Token { get; set; }

    [BsonElement("expiredAt")]
    public DateTimeOffset? ExpiredAt { get; set; }

}
