using System.Diagnostics.CodeAnalysis;
using Aruba.CmpService.BaremetalProvider.Dependencies.Configuration;
using Aruba.CmpService.BaremetalProvider.MongoDb.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Throw;

namespace Aruba.CmpService.SecurityProvider.Api.Observability;

internal sealed class AppObservabilityServicesConfigurator
    : ObservabilityServicesConfigurator
    , IObservabilityServicesConfiguratorFactory<AppObservabilityServicesConfigurator>
{
    private readonly DbSettings? dbSettings;
    private readonly MessageBusConfigurationOptions? messageBusKafkaConfigurationOptions;

    public AppObservabilityServicesConfigurator(IConfiguration configuration) :
        base(configuration)
    {
        this.messageBusKafkaConfigurationOptions = configuration.GetSection("MessageBus").Get<MessageBusConfigurationOptions>() ?? throw new ArgumentNullException(nameof(messageBusKafkaConfigurationOptions)); ;
        this.dbSettings = configuration.GetSection("MongoDB").Get<DbSettings>() ?? throw new ArgumentNullException(nameof(dbSettings)); ;
    }
    public static AppObservabilityServicesConfigurator Create([NotNull] WebApplicationBuilder builder, [NotNull] object[] configuratorParameters)
        => new(builder.Configuration);

    public override void ConfigureHealthChecksBuilder([NotNull] IHealthChecksBuilder builder)
    {
        this.dbSettings.ThrowIfNull();
        this.dbSettings.ConnectionString.ThrowIfNull();
        this.dbSettings.NameDb.ThrowIfNull();
        this.messageBusKafkaConfigurationOptions.ThrowIfNull();
        base.ConfigureHealthChecksBuilder(builder);


        builder.AddCheck("self", _ => HealthCheckResult.Healthy(), tags: [HealthCheckTags.Liveness]);

        //Mongo DB
        builder.AddMongoDb(
        this.dbSettings.ConnectionString,
        this.dbSettings.NameDb,
        name: "mongodb",
        timeout: TimeSpan.FromMilliseconds(dbSettings.HealthCheckTimeoutInMs),
        tags: new[] { HealthCheckTags.Dependencies, HealthCheckTags.Startup });

        //Server kafka
        var servers = this.messageBusKafkaConfigurationOptions!.Servers.Select(s => s.Value).ToList();

        foreach (var server in servers)
        {
            var producerConfig = new ProducerConfig()
            {
                BootstrapServers = server.BootstrapServers,

                SecurityProtocol = server.SecurityProtocol,
                SaslMechanism = server.SaslMechanism,

                SaslUsername = server.SaslUsername,
                SaslPassword = server.SaslPassword,
                SslCaLocation = server.SslCaLocation,
                SslCertificateLocation = server.SslCertificateLocation,
                ClientId = null,
                TransactionalId = null,
            };

            builder.AddKafka(producerConfig, server.HealthCheckTopic, server.ObservabilityName, timeout: TimeSpan.FromMilliseconds(server.HealthCheckTimeoutInMs), tags: new[] { HealthCheckTags.Dependencies, HealthCheckTags.Startup });
        }
    }
}


