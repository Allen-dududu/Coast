namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class Saga
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        /// <param name="steps">Saga Steps.</param>
        public Saga(IEnumerable<ISagaRequestBody> steps)
        {
            SageSteps.AddRange(steps.Select(i => new SagaStep(Id, i)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        public Saga()
        {
        }

        public long Id { get; private set; } = SnowflakeId.Default().NextId();

        public SagaStatusEnum Status { get; private set; } = SagaStatusEnum.Creating;

        public DateTime CreateTime { get; private set; }

        public long CurrentStepId { get; private set; }

        private SagaStep CurrentStep => SageSteps.FirstOrDefault(s => s.Id == CurrentStepId);

        public List<SagaStep> SageSteps { get; private set; } = new List<SagaStep>();

        public Saga AddStep(ISagaRequestBody sagaRequest, int order = default)
        {
            SageSteps.Add(new SagaStep(Id, sagaRequest, order));
            return this;
        }

        public Saga AddStep(string stepEventName, object sagaRequest, int order = default)
        {
            SageSteps.Add(new SagaStep(Id, stepEventName, sagaRequest, order));
            return this;
        }

        public SagaEvent Start()
        {

            throw new NotImplementedException();
        }

        public SagaEvent? ProcessEvent(SagaEvent sagaEvent)
        {
            var currentStep = CurrentStep;
            if (currentStep == null)
            {
                return null;
            }

            if (sagaEvent.Succeeded)
            {
                currentStep.Status = currentStep.Status switch
                {
                    SagaStepStatusEnum.Started => SagaStepStatusEnum.Succeeded,
                    SagaStepStatusEnum.Compensating => SagaStepStatusEnum.Compensated,
                    _ => throw new InvalidOperationException()
                };
            }
            else
            {
                if (currentStep.Status == SagaStepStatusEnum.Started)
                {
                    CancelSubsequentSteps();
                }
                currentStep.Status = currentStep.Status switch
                {
                    SagaStepStatusEnum.Started => SagaStepStatusEnum.Failed,
                    SagaStepStatusEnum.Compensating => SagaStepStatusEnum.Failed,
                    _ => throw new InvalidOperationException()
                };
            }

            SagaEvent? firingEvent = null;
            switch (currentStep.Status)
            {
                case SagaStepStatusEnum.Succeeded:
                    firingEvent = GoNext()?.GetStepEvent(this.Id);
                    break;
                case SagaStepStatusEnum.Compensated:
                case SagaStepStatusEnum.Failed:
                    firingEvent = GoPrevious()?.GetStepCompensateEvent(this.Id);
                    break;
            }

            UpdateSagaStatus();
            MarkProcessed(sagaEvent);

            return firingEvent ?? null;
        }
    }
}
