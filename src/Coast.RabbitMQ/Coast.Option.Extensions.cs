using Coast.Core;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.RabbitMQ
{

    public static class CoastOptionsExtensions
    {
        public static CoastOptions UseRabbitMQ(this CoastOptions options, string hostName)
        {
            return options.UseRabbitMQ(() => { return new ConnectionFactory() { HostName = hostName }; });
        }

        public static CoastOptions UseRabbitMQ(this CoastOptions options, Func<ConnectionFactory> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            options.RegisterExtension(new RabbitMQCapOptionsExtension(func()));

            return options;
        }
    }
}
