namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.EventBus;
    using Coast.Core.Util;
    using Newtonsoft.Json;

    public class SagaStep
    {
        public SagaStep(long correlationId, IEventRequestBody sagaRequestBody, bool hasCompensation = false, int executeOrder = int.MaxValue)
        {
            if (executeOrder < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(executeOrder), "executeOrder should be positive integer or 0");
            }

            CorrelationId = correlationId;
            ExecuteOrder = executeOrder;
            EventName = sagaRequestBody.GetType().Name;
            RequestBody = JsonConvert.SerializeObject(sagaRequestBody);
            HasCompensation = hasCompensation;
        }

        public SagaStep(long correlationId, string eventName, object sagaRequestBody, bool hasCompensation = false, int executeOrder = int.MaxValue)
        {
            if (executeOrder < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(executeOrder), "executeOrder should be positive integer or 0");
            }

            CorrelationId = correlationId;
            EventName = eventName;
            ExecuteOrder = executeOrder;
            RequestBody = JsonConvert.SerializeObject(sagaRequestBody);
            HasCompensation = hasCompensation;
        }

        /// <summary>
        /// Gets the Id of SagaStep.
        /// </summary>
        public long Id { get; private set; } = SnowflakeId.Default().NextId();

        /// <summary>
        /// Gets or sets the Id of Saga.
        /// </summary>
        public long CorrelationId { get; set; }

        public bool HasCompensation { get; protected set; }

        public SagaStepStateEnum State { get; set; } = SagaStepStateEnum.Awaiting;

        public string RequestBody { get; protected set; }

        public string EventName { get; protected set; }

        public string? FailedReason { get; protected set; }

        public DateTime CreationTime { get; protected set; }

        public DateTime UpdateTime { get; protected set; }

        /// <summary>
        /// Gets or sets the order in which saga step is executed.
        /// </summary>
        public int ExecuteOrder { get; set; }

        public SagaEvent GetStepEvents(long sagaId)
        {
            return new SagaEvent
            {
                EventName = EventName,
                SagaStepId = Id,
                EventType = TransactionStepTypeEnum.Commit,
                CorrelationId = CorrelationId,
                RequestBody = RequestBody,
                TransactionType = TransactionTypeEnum.Saga,
                DomainName = Const.DomainName,
            };
        }

        public SagaEvent? GetStepCompensateEvents(long sagaId)
        {
            if (!HasCompensation)
            {
                return null;
            }

            return new SagaEvent
            {
                EventName = EventName,
                SagaStepId = Id,
                EventType = TransactionStepTypeEnum.Compensate,
                CorrelationId = CorrelationId,
                RequestBody = RequestBody,
                TransactionType = TransactionTypeEnum.Saga,
                DomainName = Const.DomainName,
            };
        }
    }
}
