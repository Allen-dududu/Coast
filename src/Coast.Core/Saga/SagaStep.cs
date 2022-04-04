namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.EventBus;
    using Newtonsoft.Json;

    public class SagaStep
    {
        public SagaStep(long correlationId, ISagaRequestBody sagaRequestBody, int executeOrder = default)
        {
            CorrelationId = correlationId;
            ExecuteOrder = executeOrder;
            EventName = sagaRequestBody.GetType().Name;
            RequestBody = JsonConvert.SerializeObject(sagaRequestBody);
        }

        public SagaStep(long correlationId, string eventName, object sagaRequestBody, int executeOrder = default)
        {
            CorrelationId = correlationId;
            EventName = eventName;
            ExecuteOrder = executeOrder;
            RequestBody = JsonConvert.SerializeObject(sagaRequestBody);
        }

        /// <summary>
        /// the Id of SagaStep.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// the Id of Saga.
        /// </summary>
        public long CorrelationId { get; set; }

        public SagaStepTypeEnum StepType { get; protected set; }

        public SagaStepStatusEnum Status { get; protected set; } = SagaStepStatusEnum.Awaiting;

        public string RequestBody { get; protected set; }

        public string EventName { get; protected set; }

        public string? FailedReason { get; protected set; }

        public DateTime CreateTime { get; protected set; }

        public DateTime PublishedTime { get; protected set; }

        /// <summary>
        /// The order in which saga step is executed.
        /// </summary>
        public int ExecuteOrder { get;set; }

        public abstract SagaEvent GetStepEvent();

        protected abstract (string, string) GetStepEventDefinitionInternal();

        protected virtual (string?, string?) GetStepCompensateEventInternal() => (null, null);

        public bool RequiresCompensate
        {
            get
            {
                var (type, parameters) = GetStepCompensateEventInternal();
                return !string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(parameters);
            }
        }

        public SagaEvent GetStepEvent(Guid sagaId)
        {
            var (stepEventTypeName, stepEventParameters) = GetStepEventDefinitionInternal();

            return new SagaEvent
            {
                EventName = EventName,
                SagaStepId = Id,
                EventType = SagaStepTypeEnum.Commit,
                CorrelationId = CorrelationId,
                Payload = stepEventParameters
            };
        }

        public SagaEvent? GetStepCompensateEvent(Guid sagaId)
        {
            var (stepCompensateEventTypeName, stepCompensateEventParameters) = GetStepCompensateEventInternal();
            if (string.IsNullOrEmpty(stepCompensateEventTypeName) && string.IsNullOrEmpty(stepCompensateEventParameters))
            {
                return null;
            }

            return new SagaEvent
            {
                SagaStepId = Id,
                ServiceName = ServiceName,
                EventType = stepCompensateEventTypeName,
                SagaId = sagaId,
                Payload = stepCompensateEventParameters
            };
        }
    }
}
