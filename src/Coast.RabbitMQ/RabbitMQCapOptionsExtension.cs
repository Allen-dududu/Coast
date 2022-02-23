using Coast.Core;
using Coast.Core.EventBus;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.RabbitMQ
{
    internal class RabbitMQCapOptionsExtension : ICoastOptionsExtension
    {
        private readonly ConnectionFactory _connectionFactory;
        public RabbitMQCapOptionsExtension(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IConnectionFactory>(_connectionFactory);
            services.AddSingleton<IRabbitMQPersistentConnection, DefaultRabbitMQPersistentConnection>();
            services.AddSingleton<IEventBus, EventBusRabbitMQ>();

        }
    }
}
