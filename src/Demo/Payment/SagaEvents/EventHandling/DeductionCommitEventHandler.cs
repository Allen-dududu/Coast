using Coast.Core;
using Payment.SagaEvents.Events;

namespace Payment.SagaEvents.EventHandling
{
    public class DeductionCommitEventHandler : ISagaHandler<DeductionCommitEvent>
    {
        public Task CancelAsync(DeductionCommitEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(DeductionCommitEvent @event)
        {
            Console.WriteLine("Deduction commit");
            return Task.CompletedTask;
        }
    }
}
