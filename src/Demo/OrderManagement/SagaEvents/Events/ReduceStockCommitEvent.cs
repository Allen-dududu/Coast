using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class ReduceStockCommitEvent : EventRequestBody
    {
        public int Number { get; set; } = 0;
    }
}
