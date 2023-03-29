using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class DeductionCommitEvent : EventRequestBody
    {
        public long Money { get; set; }
    }
}
