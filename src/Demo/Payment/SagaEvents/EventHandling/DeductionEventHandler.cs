using Coast.Core;
using Payment.SagaEvents.Events;

namespace Payment.SagaEvents.EventHandling
{
    public class DeductionEventHandler : ISagaHandler<DeductionEvent>
    {
        public Task CancelAsync(DeductionEvent @event)
        {
            Console.WriteLine($"Cancel Deduction {@event.Money}");
            return Task.CompletedTask;
        }

        public Task CommitAsync(DeductionEvent @event)
        {
            Console.WriteLine($"Commit Deduction {@event.Money}");
            return Task.CompletedTask;
        }
    }
}
