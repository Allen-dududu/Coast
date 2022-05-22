using Coast.Core;

namespace Payment.SagaEvents.Events
{
    public class DeductionEvent : EventRequestBody
    {
        public long Money { get; set; }
    }
}
