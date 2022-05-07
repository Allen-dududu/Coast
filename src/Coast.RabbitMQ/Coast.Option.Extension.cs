namespace Coast.RabbitMQ
{
    using Coast.Core;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using global::RabbitMQ.Client;
    using System;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class CoastOptionsExtension
    {
        public static CoastOptions UseRabbitMQ(this CoastOptions options, string hostName, string subscriptionClientName, int retryCount)
        {
            return options.UseRabbitMQ(new ConnectionFactory() { HostName = hostName,
                DispatchConsumersAsync = true
            }, subscriptionClientName, retryCount);
        }

        public static CoastOptions UseRabbitMQ(this CoastOptions options, ConnectionFactory connectionFactory, string subscriptionClientName, int retryCount)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            options.RegisterExtension(serviceCollection =>
            {
                serviceCollection.TryAddSingleton<IConnectionFactory>(connectionFactory);
                serviceCollection.TryAddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
                serviceCollection.TryAddSingleton<IEventBus>(s =>
                {
                    var pc = s.GetRequiredService<IRabbitMQPersistentConnection>();
                    var log = s.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var subsManager = s.GetRequiredService<IEventBusSubscriptionsManager>();
                    var processEvent = s.GetRequiredService<IProcessSagaEvent>();
                    return new EventBusRabbitMQ(pc, log, s, subsManager, subscriptionClientName, retryCount, processEvent);
                });
            });

            return options;
        }
    }
}
