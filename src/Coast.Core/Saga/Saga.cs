namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Saga
    {
        private List<(SagaStep, SagaStep)> sageSteps { get; set; } = new List<(SagaStep, SagaStep)>();

        public Saga(IEnumerable<(SagaStep, SagaStep)> steps)
        {
            sageSteps.AddRange(steps);
        }

        public Saga()
        {

        }

        public long Id { get; set; }

        public SagaStatus Status { get; set; } = SagaStatus.Created;
    }
}
