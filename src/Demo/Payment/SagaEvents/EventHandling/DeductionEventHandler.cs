using Coast.Core;
using Payment.SagaEvents.Events;

namespace Payment.SagaEvents.EventHandling
{
    public class DeductionEventHandler : ISagaHandler<DeductionRequest>
    {
        public Task CancelAsync(DeductionRequest @event)
        {
            Console.WriteLine($"Cancel Deduction {@event.Money}");
            return Task.CompletedTask;
        }

        public Task CommitAsync(DeductionRequest @event)
        {
            Console.WriteLine($"Commit Deduction {@event.Money}");
            return Task.CompletedTask;
        }
    }
}
