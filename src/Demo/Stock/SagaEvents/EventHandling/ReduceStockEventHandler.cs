using Coast.Core;
using Stock.SagaEvents.Events;

namespace Stock.SagaEvents.EventHandling
{
    public class ReduceStockEventHandler : ISagaHandler<ReduceStockRequest>
    {
        private readonly IBarrierService barrierService;

        public ReduceStockEventHandler(IBarrierService barrierService)
        {
            this.barrierService = barrierService;
        }
        public Task CancelAsync(ReduceStockRequest @event)
        {
            Console.WriteLine($"Cancel ReduceStock{@event.Number}");
            return Task.CompletedTask;
        }

        public Task CommitAsync(ReduceStockRequest @event)
        {
            Console.WriteLine($"ReduceStock {@event.Number}");
            if(@event.Number == 2)
            {
                throw new Exception("error");
            }
            return Task.CompletedTask;
        }
    }
}
