namespace Coast.Core.Saga
{
    using Coast.Core.EventBus;

    public class SagaEvent : IntegrationEvent
    {
        public long CorrelationId { get; set; }

        public long SagaStepId { get; set;}

        public SagaStepTypeEnum EventType { get; set; }

    }
}
