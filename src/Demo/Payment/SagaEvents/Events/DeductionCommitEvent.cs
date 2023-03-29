using Coast.Core;

namespace Payment.SagaEvents.Events
{
    public class DeductionCommitEvent : EventRequestBody
    {
        public long Money { get; set; }
    }
}
