using System.Reflection;
using Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Messages;
using Aruba.CmpService.BaremetalProvider.Abstractions.MessageBus;
using Aruba.CmpService.BaremetalProvider.Abstractions.Messages.Notifications;
using Aruba.CmpService.BaremetalProvider.Dependencies;
using Aruba.CmpService.BaremetalProvider.Dependencies.Configuration;
using Aruba.CmpService.ResourceProvider.Common.Messages.v1;
using Aruba.MessageBus;
using Aruba.MessageBus.Builder;
using Aruba.MessageBus.Configuration;
using Aruba.MessageBus.Confluent.Kafka.Configuration.Builder;
using Aruba.MessageBus.Confluent.Kafka.Transport;
using Aruba.MessageBus.Hosting;
using Aruba.MessageBus.MessageHandlers;
using Aruba.MessageBus.Metadata;
using Aruba.MessageBus.Transactions;
using Aruba.MessageBus.Transport;
using Aruba.MessageBus.UseCases;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aruba.CmpService.BaremetalProvider.Dependencies;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute(Justification = "Integration Test: Necessità del message bus")]

public static class AbstractionsServiceCollectionExtensions
{
    internal static IServiceCollection AddAbstractions(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddMessageBus(configuration, isDevelopment);

        return services;
    }
    private static IServiceCollection AddMessageBus(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var assembly = typeof(MessageBusResponse).Assembly;
        var assemblyTypes = (assembly.GetTypes().Concat(typeof(DeploymentStatusChanged).Assembly.GetTypes())).ToArray();
        var messageBusOptions = configuration.GetSection("MessageBus").Get<MessageBusConfigurationOptions>();
        //var auditingOptions = configuration.GetSection(nameof(AuditingOptions)).Get<AuditingOptions>();
        var distinctCeTypes = messageBusOptions.CeTypes.Select(s => s.Value).Distinct().ToHashSet();

        SetTopics(messageBusOptions);
        services.Configure<CloudEventsOptions>(o => o.DefaultSource = messageBusOptions.GroupId)
            .ConfigureMessageBus(mbus => mbus.RegisterHandlersAndUseCases(assemblyTypes));

        if (!isDevelopment)
        {
            services.ConfigureMessageBus(mbus => mbus.AddGenericHostIntegration());
        }

        services.AddMessageBus(EndpointIdentity.CreateDefault(), mb =>
        {

            mb.SetDefaultTransactionTimeout(TimeSpan.FromSeconds(30));
            mb.ConfigureTypeResolver(typeTable => Configure(typeTable, messageBusOptions.CeTypes, assemblyTypes))
               //.AddAuditingTypeResolver()
               .AddKafkaTransport(kafka =>
               {
                   foreach (var connectOption in messageBusOptions.Servers)
                   {
                       ConfigureKafkaConnect(connectOption.Key, kafka, connectOption.Value, messageBusOptions.GroupId);
                   }

                   //kafka.AddAuditingKafkaConnect(auditingOptions);
               })
               .RouteOutboundEnvelopes(KafkaRoutes)
               //.AddAuditingKafkaRoutes(auditingOptions)
               //.RegisterAuditUseCase()
               .ConfigurePersistence(pc => pc.UseSystemTransactions().UseMongoDb());

            mb.FilterInboundTransportEnvelope((context, cancellationToken) =>
            {
                // use context to mutate the inbound transport envelope
                // this delegate returns a ValueTask, and so may contain async code
                // return false to ignore a message.
                context.Headers.TryGetValue("ce_type", out var type);
                return ValueTask.FromResult(distinctCeTypes.Contains(type));
            });
        })
        .Configure<MessageBusOptions>(o => o.StrictMode = false);
        return services;
    }

    private static void SetTopics(MessageBusConfigurationOptions messageBusConfiguration)
    {
        var topics = messageBusConfiguration.Servers.Select(s => s.Value)
            .SelectMany(sm => sm.Topics)
            .ToList();

        var topicProperties = typeof(Topics).GetProperties(BindingFlags.Static | BindingFlags.Public).ToList();
        foreach (var topic in topics)
        {
            var topicProperty = topicProperties.FirstOrDefault(f => f.Name == topic.Key);
            if (topicProperty != null)
            {
                topicProperty.SetValue(null, topic.Value);
            }
        }

    }

    private static void RegisterHandlersAndUseCases(this IMessageBusBuilder builder, Type[] assemblyTypes)
    {

        //Registro gli handlers
        var registerMethod = typeof(MessageHandlerIMessageBusBuilderExtensions).GetMethod(nameof(MessageHandlerIMessageBusBuilderExtensions.RegisterHandler));
        builder.RegisterSingleType(assemblyTypes, typeof(MessageHandler<>), registerMethod);

        //Registro gli usecases
        registerMethod = typeof(UseCaseIMessageBusBuilderExtensions).GetMethod(nameof(UseCaseIMessageBusBuilderExtensions.RegisterUseCase));
        builder.RegisterSingleType(assemblyTypes, typeof(UseCase<,>), registerMethod);
    }

    private static void RegisterSingleType(this IMessageBusBuilder builder, Type[] assemblyTypes, Type baseType, MethodInfo registerMethod)
    {
        var typesToFilter = assemblyTypes.Where(w => !w.IsAbstract && w.BaseType != null).ToList();
        var typesToRegister = new Dictionary<Type, Type>();
        foreach (var typeToFilter in typesToFilter)
        {
            var typeToRegister = GetBaseType(typeToFilter.BaseType, baseType);
            if (typeToRegister != null)
            {
                typesToRegister.Add(typeToFilter, typeToRegister);
            }
        }
        //Prendo il metoo registerHandler del builder
        foreach (var typeToRegister in typesToRegister)
        {
            //preno il tipo generic
            var handlerTypes = new List<Type>() { typeToRegister.Key }.Concat(typeToRegister.Value.GetGenericArguments()).ToArray();
            var registerMethodInstance = registerMethod.MakeGenericMethod(handlerTypes);
            registerMethodInstance.Invoke(null, new object[1] { builder });
        }

    }

    private static Type GetBaseType(Type type, Type basetype)
    {
        while (type != null)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == basetype)
            {
                break;
            }
            else
            {
                type = type.BaseType;
            }
        }

        return type;
    }

    private static void Configure(TypeTable typeTable, CeTypes ceTypes, Type[] assemblyTypes)
    {

        foreach (var ceType in ceTypes)
        {
            ArgumentNullException.ThrowIfNull(ceType.Value);
            var type = assemblyTypes.Where(w => w.Name == ceType.Key).FirstOrDefault();
            if (type == null)
            {
                throw new ArgumentException($"type {ceType.Key} not found");
            }
            typeTable.Add(ceType.Value, type);
        }
    }

    private static void ConfigureKafkaConnect(string name, KafkaTransportComponentConfigurationBuilder kafka, MessageBusServer server, string groupId)
    {
        kafka.EnableDetailedLogging().Connect(name, connect =>
        {
            connect.UseBootstrapServers(server.BootstrapServers!)
                   .UseConfiguration(c =>
                   {
                       if (string.IsNullOrWhiteSpace(server.SaslUsername)
                           || string.IsNullOrWhiteSpace(server.SaslPassword)
                           || string.IsNullOrWhiteSpace(server.SslCaLocation)
                           || string.IsNullOrWhiteSpace(server.SslCertificateLocation))
                       {
                           return;
                       }

                       c.SecurityProtocol = server.SecurityProtocol;
                       c.SaslMechanism = server.SaslMechanism;
                       c.SaslUsername = server.SaslUsername;
                       c.SaslPassword = server.SaslPassword;
                       c.SslCaLocation = server.SslCaLocation;
                       c.SslCertificateLocation = server.SslCertificateLocation;
                   })
                   .Consume(groupId, groupid => groupid.FromTopics(server.Topics.Select(s => s.Value).ToArray())
                    .UseConfiguration(c => c.PartitionAssignmentStrategy = PartitionAssignmentStrategy.RoundRobin));
        });
    }

    private static void KafkaRoutes(RoutingTable routingTable)
    {
        routingTable.AddPolicy((headers, body, routes) =>
        {
            routes.TryAdd<DeploymentStatusChanged>(Topics.DeploymentEvents!, o => o.DeploymentId, body);
            routes.TryAdd<Notification>(Topics.NotificationEvents!, o => o.Id, body);
            routes.TryAdd<IResourceMessage>(Topics.ResourcesEvents!, o => o.DeploymentId, body);
        });
    }

    private static void TryAdd<T>(this IList<IRoute> routes, string topic, Func<T, string?> func, object body)
    {
        if (body is T message)
        {
            routes.Add(new KafkaRoute(topic: topic, func(message)));
        }
    }
}
