namespace Coast.Core.EventBus
{
    using System;
    using Coast.Core.Util;

    public class IntegrationEvent
    {
        private string? eventName;

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

        public string EventName
        {
            get
            {
                return this.eventName ?? this.GetType().Name;
            }

            set
            {
                this.eventName = value;
            }
        }

        public TransactionTypeEnum TransactionType { get; set; }

        public bool Succeeded { get; set; }

        public short FailedCount { get; set; }

        public string DomainName { get; set; } = CoastConstant.DomainName;

        public string CallBackEventName { get; set; }

        public long CorrelationId { get; set; }

        public long StepId { get; set; }

        public TransactionStepTypeEnum StepType { get; set; }

        public bool IsCallBack { get; set; }

        public bool NotAllowedFail
        {
            get; set;
        }
    }
}
