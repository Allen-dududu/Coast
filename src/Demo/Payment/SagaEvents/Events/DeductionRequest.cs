using Coast.Core;

namespace Payment.SagaEvents.Events
{
    public class DeductionRequest : EventRequestBody
    {
        public long Money { get; set; }
    }
}
