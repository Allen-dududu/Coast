using Coast.Core;

namespace Saga_RabbitMQ_PostgreSql
{
    public class TradeInEventHandle : ISagaHandler<TradeInRequest>
    {
        public Task Cancel(SagaEvent<TradeInRequest> @event)
        {
            throw new NotImplementedException();
        }

        public Task Commit(SagaEvent<TradeInRequest> @event)
        {
            throw new NotImplementedException();
        }
    }
}
