using Coast.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.RabbitMQ
{

    public static class CoastOptionsExtensions
    {
        public static CoastOptions UseRabbitMQ(this CoastOptions options)
        {
            options.RegisterExtension(new InMemoryCapOptionsExtension());
            return options;
        }
    }
}
