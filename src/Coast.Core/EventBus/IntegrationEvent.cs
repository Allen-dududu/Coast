namespace Coast.Core.EventBus
{
    using Coast.Core.Util;
    using System;

    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = SnowflakeId.Default().NextId();
            CreationDate = DateTime.UtcNow;
        }

        public IntegrationEvent(long id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
        }

        public long Id { get; private set; }

        public DateTime CreationDate { get; private set; }

        public string EventName { get; set; }

        public TransactionTypeEnum TransactionType { get; set; }

        public bool Succeeded { get; set; }

        public string DomainName { get; set; } = CoastConstant.DomainName;

        public string CallBackEventName { get; set; }

        public long CorrelationId { get; set; }

        public long StepId { get; set; }

        public TransactionStepTypeEnum StepType { get; set; }

    }
}
