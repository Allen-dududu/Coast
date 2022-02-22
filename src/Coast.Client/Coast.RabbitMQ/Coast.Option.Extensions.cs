using Coast.Core;
using Coast.Core.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.RabbitMQ
{

    public static class CoastOptionsExtensions
    {
        public static CoastOptions UseRabbitMQ(this CoastOptions options, string hostName, string subscriptionClientName, int retryCount)
        {
            return options.UseRabbitMQ(new ConnectionFactory() { HostName = hostName }, subscriptionClientName, retryCount);
        }

        public static CoastOptions UseRabbitMQ(this CoastOptions options, ConnectionFactory connectionFactory, string subscriptionClientName, int retryCount)
        {
            if (connectionFactory == null)
            {
                throw new ArgumentNullException(nameof(connectionFactory));
            }

            options.RegisterExtension( serviceCollection => {
                serviceCollection.AddSingleton<ConnectionFactory>(connectionFactory);
                serviceCollection.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
                serviceCollection.AddSingleton<IEventBus>(s => {
                    var pc = s.GetRequiredService<IRabbitMQPersistentConnection>();
                    var log = s.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var subsManager = s.GetRequiredService<IEventBusSubscriptionsManager>();
                    return new EventBusRabbitMQ(pc, log, s, subsManager, subscriptionClientName, retryCount);
                });
            });

            return options;
        }
    }
}
