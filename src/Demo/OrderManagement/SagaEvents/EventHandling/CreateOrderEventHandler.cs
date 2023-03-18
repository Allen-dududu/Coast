using Coast.Core;
using OrderManagement.SagaEvents.Events;

namespace OrderManagement.SagaEvents.EventHandling
{
    public class CreateOrderEventHandler : ISagaHandler<CreateOrderEvent>
    {
        public Task CancelAsync(CreateOrderEvent @event)
        {
            Console.WriteLine("Cancel buy " + @event.OrderName);
            return Task.CompletedTask;
        }

        public Task CommitAsync(CreateOrderEvent @event)
        {
           Console.WriteLine("buy " + @event.OrderName);
            return Task.CompletedTask;
        }
    }
}
