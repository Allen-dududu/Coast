namespace Coast.RabbitMQ
{
    using System;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus;
    using global::RabbitMQ.Client;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;

    public static class CoastOptionsExtension
    {
        public static CoastOptions UseRabbitMQ(this CoastOptions options, string hostName, string subscriptionClientName, int retryCount)
        {
            return options.UseRabbitMQ(new ConnectionFactory()
            {
                HostName = hostName,
                DispatchConsumersAsync = true
            }, subscriptionClientName, retryCount);
        }

        public static CoastOptions UseRabbitMQ(this CoastOptions options, ConnectionFactory connectionFactory, string? subscriptionClientName, int retryCount = 5)
        {
            if (connectionFactory is null)
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
                    var repositoryFactory = s.GetRequiredService<IRepositoryFactory>();
                    var option = s.GetRequiredService<CoastOptions>();

                    return new EventBusRabbitMQ(pc, log, s, subsManager, processEvent, repositoryFactory, option, subscriptionClientName, retryCount);
                });
            });

            return options;
        }
    }
}
