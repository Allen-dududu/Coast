using Coast.Core;

namespace Stock.SagaEvents.Events
{
    public class ReduceStockRequest : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
