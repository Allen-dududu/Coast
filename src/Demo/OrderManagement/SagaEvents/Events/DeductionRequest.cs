using Coast.Core;

namespace OrderManagement.SagaEvents.Events
{
    public class DeductionRequest : EventRequestBody
    {
        public long Money { get; set; }
    }
}
