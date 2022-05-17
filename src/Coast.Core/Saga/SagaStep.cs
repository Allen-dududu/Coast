namespace Coast.Core
{
    using System;
    using System.Text.Json;
    using Coast.Core.Util;

    public class SagaStep
    {
        internal SagaStep()
        {

        }

        public SagaStep(long correlationId, EventRequestBody sagaRequestBody, bool hasCompensation = false, int executionSequenceNumber = int.MaxValue)
        {
            if (executionSequenceNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(executionSequenceNumber), "ExecutionSequenceNumber should be positive integer or 0");
            }

            CorrelationId = correlationId;
            ExecutionSequenceNumber = executionSequenceNumber;
            EventName = sagaRequestBody.GetType().Name;
            RequestBody = JsonSerializer.Serialize(sagaRequestBody, sagaRequestBody.GetType());
            HasCompensation = hasCompensation;
        }

        public SagaStep(long correlationId, string eventName, object sagaRequestBody, bool hasCompensation = false, int executionSequenceNumber = int.MaxValue)
        {
            if (executionSequenceNumber < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(executionSequenceNumber), "ExecutionSequenceNumber should be positive integer or 0");
            }

            CorrelationId = correlationId;
            EventName = eventName;
            ExecutionSequenceNumber = executionSequenceNumber;
            RequestBody = JsonSerializer.Serialize(sagaRequestBody, sagaRequestBody.GetType());
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

        public string RequestBody { get; set; }

        public string EventName { get; protected set; }

        public string? FailedReason { get; protected set; }

        public DateTime CreationTime { get; protected set; }

        public DateTime UpdateTime { get; protected set; }

        /// <summary>
        /// Gets or sets the order in which saga step is executed.
        /// </summary>
        public int ExecutionSequenceNumber { get; set; }

        public SagaEvent GetStepEvents(long sagaId)
        {
            return new SagaEvent
            {
                EventName = EventName,
                StepId = Id,
                StepType = TransactionStepTypeEnum.Commit,
                CorrelationId = CorrelationId,
                RequestBody = RequestBody,
                TransactionType = TransactionTypeEnum.Saga,
                CallBackEventName = CoastConstant.DomainName + CoastConstant.CallBackEventSuffix,
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
                StepId = Id,
                StepType = TransactionStepTypeEnum.Compensate,
                CorrelationId = CorrelationId,
                RequestBody = RequestBody,
                TransactionType = TransactionTypeEnum.Saga,
                CallBackEventName = CoastConstant.DomainName + CoastConstant.CallBackEventSuffix,
            };
        }
    }
}
