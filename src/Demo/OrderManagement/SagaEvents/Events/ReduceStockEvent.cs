using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class ReduceStockEvent : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
