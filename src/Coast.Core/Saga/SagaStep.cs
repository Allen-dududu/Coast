namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.EventBus;
    using Newtonsoft.Json;

    public abstract class SagaStep
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public SagaStepType HandleEventName { get; set; }

        public SagaStepStatus Status { get; set; } = SagaStepStatus.Awaiting;

        public ISagaRequestBody RequestBody { get; set; }

        public string FailedReason { get; set; }

        public abstract SagaStepEvent GetStepEvent();
    }
}
