using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class DeductionEvent : EventRequestBody
    {
        public long Money { get; set; }
    }
}
