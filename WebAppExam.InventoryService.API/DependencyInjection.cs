using KafkaFlow;
using KafkaFlow.Serializer;
using KafkaFlow.Retry;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAppExam.InventoryService.Infrastructure.Common;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Infrastructure.Consumers;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderCanceledConsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;
using WebAppExam.InventoryService.Domain.Exceptions;
using MongoDB.Driver;
using Confluent.Kafka;
using StackExchange.Redis;

namespace WebAppExam.InventoryService.API;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Kafka
        var kafkaBrokers = configuration.GetSection("KafkaConfig:Brokers").Get<string[]>()
                           ?? new[] { KafkaSettings.DefaultBroker };

        services.AddKafka(kafka => kafka
            .UseConsoleLog()
            .AddCluster(cluster => cluster
                .WithBrokers(kafkaBrokers)
                .CreateTopicIfNotExists(KafkaTopics.OrderReplyTopic, KafkaSettings.TopicPartitions, KafkaSettings.TopicReplicationFactor)
                .CreateTopicIfNotExists(KafkaTopics.OrderUpdatedReplyTopic, KafkaSettings.TopicPartitions, KafkaSettings.TopicReplicationFactor)
                .AddProducer(
                    KafkaProducers.OrderReplyProducer,
                    producer => producer
                        .DefaultTopic(KafkaTopics.OrderReplyTopic)
                        .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
                .AddProducer(
                    KafkaProducers.OrderUpdatedReplyProducer,
                    producer => producer
                        .DefaultTopic(KafkaTopics.OrderUpdatedReplyTopic)
                        .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
                .AddProducer(
                    KafkaProducers.OrderCanceledReplyProducer,
                    producer => producer
                        .DefaultTopic(KafkaTopics.OrderCanceledReplyTopic)
                        .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
                )
                .AddConsumer(consumer => consumer
                    .Topic(KafkaTopics.OrderEventTopic)
                    .WithGroupId(KafkaConsumerGroups.InventoryOrderGroup)
                    .WithWorkersCount(KafkaSettings.ConsumerWorkersCount)
                    .WithBufferSize(KafkaSettings.ConsumerBufferSize)
                    .AddMiddlewares(middlewares => middlewares
                        .AddDeserializer<JsonCoreDeserializer, MessageTypeResolver>()
                        .RetrySimple(retry => retry
                            .Handle<DatabaseOperationException>()
                            .Handle<MongoException>()
                            .Handle<KafkaException>()
                            .Handle<RedisException>()
                            .Handle<RedisTimeoutException>()
                            .Handle<RedisConnectionException>()
                            .TryTimes(3)
                            .WithTimeBetweenTriesPlan((retryCount) =>
                                TimeSpan.FromMinutes(5)
                            )
                        )
                        .AddTypedHandlers(h => h
                            .WithHandlerLifetime(InstanceLifetime.Scoped)
                            .AddHandler<OrderCreatedConsumer>()
                            .AddHandler<OrderUpdatedConsumer>()
                            .AddHandler<OrderDeletedConsumer>()
                            .AddHandler<OrderCanceledConsumer>()
                        )
                    )
                )
                // .AddConsumer(consumer => consumer
                //     .Topic(KafkaTopics.OrderDeletedTopic)
                //     .WithGroupId(KafkaConsumerGroups.InventoryOrderDeletedGroup)
                //     .WithWorkersCount(KafkaSettings.ConsumerWorkersCount)
                //     .WithBufferSize(KafkaSettings.ConsumerBufferSize)
                //     .AddMiddlewares(middlewares => middlewares
                //         .AddSingleTypeDeserializer<OrderDeletedEvent, JsonCoreDeserializer>()
                //         .AddTypedHandlers(h => h
                //             .WithHandlerLifetime(InstanceLifetime.Scoped)
                //             .AddHandler<OrderDeletedConsumer>())
                //     )
                // )
                // .AddConsumer(consumer => consumer
                //     .Topic(KafkaTopics.OrderCanceledTopic)
                //     .WithGroupId(KafkaConsumerGroups.InventoryOrderCanceledGroup)
                //     .WithWorkersCount(KafkaSettings.ConsumerWorkersCount)
                //     .WithBufferSize(KafkaSettings.ConsumerBufferSize)
                //     .AddMiddlewares(middlewares => middlewares
                //         .AddSingleTypeDeserializer<OrderCanceledEvent, JsonCoreDeserializer>()
                //         .AddTypedHandlers(h => h
                //             .WithHandlerLifetime(InstanceLifetime.Scoped)
                //             .AddHandler<OrderCanceledConsumer>())
                //     )
                // )
            )
        );

        // Configure JWT Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
            };
        });

        // Configure Authorization
        services.AddAuthorization(options =>
        {
            var sharedPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAssertion(context =>
                {
                    var httpContext = context.Resource as HttpContext;
                    if (httpContext == null) return false;

                    if (httpContext.User.Identity?.IsAuthenticated == true) return true;

                    if (httpContext.Request.Headers.TryGetValue(CommonConstants.InternalKeyHeader, out var extractedKey))
                    {
                        var secretKey = configuration[CommonConstants.InternalApiKeyConfigPath];
                        return !string.IsNullOrEmpty(secretKey) && extractedKey == secretKey;
                    }

                    return false;
                })
                .Build();

            options.FallbackPolicy = sharedPolicy;
        });

        // Add gRPC services
        services.AddGrpc();
        services.AddGrpcReflection();

        services.AddControllers();

        return services;
    }
}
