using Coast.Core;

namespace Stock.SagaEvents.Events
{
    public class ReduceStockEvent : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
