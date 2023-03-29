using Coast.Core;

namespace Stock.SagaEvents.Events
{
    public class ReduceStockCommitEvent : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
