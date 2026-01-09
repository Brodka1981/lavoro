namespace Aruba.CmpService.BaremetalProvider.Dependencies.Configuration;
public class MessageBusConfigurationOptions
{
    public string GroupId { get; set; }
    public Dictionary<string, MessageBusServer> Servers { get; set; }
    public CeTypes CeTypes { get; set; }

}
