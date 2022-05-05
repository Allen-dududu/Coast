using Coast.Core;

namespace Saga_RabbitMQ_PostgreSql
{
    public class TradeOutRequest : IEventRequestBody
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long Amount { get; set; }
    }
}
