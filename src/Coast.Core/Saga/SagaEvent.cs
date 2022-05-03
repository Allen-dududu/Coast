namespace Coast.Core
{
    using System.Collections.Generic;
    using Coast.Core.EventBus;

    public class SagaEvent : IntegrationEvent
    {
        public long CorrelationId { get; set; }

        public long SagaStepId { get; set; }

        public TransactionStepTypeEnum EventType { get; set; }

        public string RequestBody { get; set; }

        public string ErrorMessage { get; set; }

        public bool Succeeded { get; set; }

        public IDictionary<string, string> Headers { get; set; }
    }
}
