using System.Xml.Linq;
using Aruba.CmpService.BaremetalProvider.MongoDb.Entities;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Driver;

namespace Aruba.CmpService.BaremetalProvider.MongoDb.Repositories;
public class XmlRepository :
    IXmlRepository
{
    private readonly Func<BaremetalProviderDbContext> dbContextLoader;

    private IMongoCollection<DataProtectionKeyEntity> Collection => this.dbContextLoader().DataProtectionKeys;

    public XmlRepository(Func<BaremetalProviderDbContext> dbContextLoader)
    {
        this.dbContextLoader = dbContextLoader;
    }
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return this.Collection.FindSync(Builders<DataProtectionKeyEntity>.Filter.Empty)
                         .ToList()
                         .Select(key => XElement.Parse(key.Xml ?? throw new InvalidOperationException($"Missing xml data for key {key.Id}")))
                         .ToList()
                         .AsReadOnly();
    }

    /// <inheritdoc />
    public void StoreElement(XElement element, string friendlyName)
    {
        var newKey = new DataProtectionKeyEntity()
        {
            FriendlyName = friendlyName,
            Xml = element.ToString(SaveOptions.DisableFormatting)
        };

        this.Collection.InsertOne(newKey);
    }
}
