using Coast.Core;
using Stock.SagaEvents.Events;

namespace Stock.SagaEvents.EventHandling
{
    public class ReduceStockEventHandler : ISagaHandler<ReduceStockRequest>
    {
        public Task CancelAsync(ReduceStockRequest @event)
        {
            Console.WriteLine($"Cancel ReduceStock{@event.Number}");
            return Task.CompletedTask;
        }

        public Task CommitAsync(ReduceStockRequest @event)
        {
            throw new NotImplementedException();
            Console.WriteLine($"ReduceStock {@event.Number}");
            return Task.CompletedTask;
        }
    }
}
