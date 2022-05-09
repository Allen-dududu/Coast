using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class ReduceStockRequest : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
