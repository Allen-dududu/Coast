using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class CreateOrderCommitEvent : EventRequestBody
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        public string OrderName { get; set; }

        public int Number { get; set; }
    }
}
