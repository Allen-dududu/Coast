using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class CreateOrderEvent : EventRequestBody
    {
        public string OrderId { get; set;} = Guid.NewGuid().ToString();
        public string OrderName { get; set;}
    }
}
