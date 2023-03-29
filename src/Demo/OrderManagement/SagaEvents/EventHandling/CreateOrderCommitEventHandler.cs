using Coast.Core;
using OrderManagement.SagaEvents.Events;

namespace OrderManagement.SagaEvents.EventHandling
{
    public class CreateOrderCommitEventHandler : ISagaHandler<CreateOrderCommitEvent>
    {
        public Task CancelAsync(CreateOrderCommitEvent @event)
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CreateOrderCommitEvent @event)
        {
            Console.WriteLine("CreateOrder Commit");

            return Task.CompletedTask;
        }
    }
}
