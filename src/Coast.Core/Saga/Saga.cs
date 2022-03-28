namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Saga
    {
        private List<(SagaStep, SagaStep)> sageSteps { get; set; } = new List<(SagaStep, SagaStep)>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        /// <param name="steps">Saga Steps.</param>
        public Saga(IEnumerable<(SagaStep, SagaStep)> steps)
        {
            sageSteps.AddRange(steps);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        public Saga()
        {

        }

        public long Id { get; set; }

        public SagaStatus Status { get; set; } = SagaStatus.Created;
    }
}
