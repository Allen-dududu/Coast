using Coast.Core;
using Stock.SagaEvents.Events;

namespace Stock.SagaEvents.EventHandling
{
    public class ReduceStockCommitEventHandler : ISagaHandler<ReduceStockCommitEvent>
    {
        public Task CancelAsync(ReduceStockCommitEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(ReduceStockCommitEvent @event)
        {
            Console.WriteLine("ReduceStock commit");
            return Task.CompletedTask;
        }
    }
}
