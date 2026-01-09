using Confluent.Kafka;

namespace Aruba.CmpService.BaremetalProvider.Dependencies.Configuration;

public class MessageBusServer
{
    public string? ObservabilityName { get; set; }
    public string? BootstrapServers { get; set; }
    public string? HealthCheckTopic { get; set; }
    public string? SaslUsername { get; set; }
    public string? SaslPassword { get; set; }
    public string? SslCaLocation { get; set; }
    public string? SslCertificateLocation { get; set; }
    public Dictionary<string, string> Topics { get; set; }
    public long HealthCheckTimeoutInMs { get; set; } = 2000;
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SaslSsl;
    public SaslMechanism SaslMechanism { get; set; } = SaslMechanism.ScramSha256;

}
