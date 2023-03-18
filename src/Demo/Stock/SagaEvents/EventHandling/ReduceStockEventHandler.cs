using Coast.Core;
using Stock.SagaEvents.Events;

namespace Stock.SagaEvents.EventHandling
{
    public class ReduceStockEventHandler : ISagaHandler<ReduceStockEvent>
    {
        public ReduceStockEventHandler()
        {
        }
        public Task CancelAsync(ReduceStockEvent @event)
        {
            Console.WriteLine($"Cancel ReduceStock{@event.Number}");
            return Task.CompletedTask;
        }

        public Task CommitAsync(ReduceStockEvent @event)
        {
            Console.WriteLine($"ReduceStock {@event.Number}");

            if(@event.Number >= 2)
            {
                throw new Exception("error");
            }

            return Task.CompletedTask;
        }
    }
}
