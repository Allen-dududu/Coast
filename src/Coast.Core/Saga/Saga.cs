namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class Saga
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        /// <param name="steps">Saga Steps.</param>
        public Saga(IEnumerable<EventRequestBody> steps)
        {
            if (steps != null)
            {
                SagaSteps.AddRange(steps.Select(i => new SagaStep(Id, i)));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        public Saga()
        {
        }

        public long Id { get; private set; } = SnowflakeId.Default().NextId();

        public SagaStateEnum State { get; private set; } = SagaStateEnum.Created;

        public DateTime CreationTime { get; private set; }

        public int CurrentExecutionSequenceNumber { get; private set; }

        public string? AbortingReason { get; set; }

        internal ICollection<SagaStep> CurrentSagaStepGroup => SagaSteps.Where(s => s.ExecutionSequenceNumber == CurrentExecutionSequenceNumber).ToList();

        private List<IGrouping<int, SagaStep>> SagaStepGroups => SagaSteps.GroupBy(i => i.ExecutionSequenceNumber).OrderBy(i => i.Key).ToList();

        private List<SagaStep>? NextStepGroup
        {
            get
            {
                var idx = SagaStepGroups.FindIndex(s => s.Key == CurrentExecutionSequenceNumber);

                if (idx == -1)
                {
                    return null;
                }

                Interlocked.Increment(ref idx);
                if (idx == SagaStepGroups.Count)
                {
                    return null;
                }

                return SagaStepGroups[idx].Select(s => s).ToList();
            }
        }

        private List<SagaStep>? PreviousStepGroup
        {
            get
            {
                var idx = SagaStepGroups.FindIndex(s => s.Key == CurrentExecutionSequenceNumber);

                if (idx == -1)
                {
                    return null;
                }

                Interlocked.Decrement(ref idx);
                if (idx < 0)
                {
                    return null;
                }

                return SagaStepGroups[idx].Select(s => s).ToList();

            }
        }

        public List<SagaStep> SagaSteps { get; set; } = new List<SagaStep>();

        public Saga AddStep(EventRequestBody sagaRequest, bool hasCompensation = default, int executionSequenceNumber = int.MaxValue)
        {
            SagaSteps.Add(new SagaStep(Id, sagaRequest, hasCompensation, executionSequenceNumber));
            return this;
        }

        public Saga AddStep(string stepEventName, object sagaRequest, bool hasCompensation = default, int executionSequenceNumber = int.MaxValue)
        {
            SagaSteps.Add(new SagaStep(Id, stepEventName, sagaRequest, hasCompensation, executionSequenceNumber));
            return this;
        }

        internal List<SagaEvent> Start()
        {
            if (SagaSteps.Count == 0)
            {
                State = SagaStateEnum.Completed;
                return null;
            }

            SagaSteps = SagaSteps.OrderBy(i => i.ExecutionSequenceNumber).ToList();
            int maxExecuteOrderNumber = default;

            for (int i = 0; i < SagaSteps.Count; i++)
            {
                if (SagaSteps[i].ExecutionSequenceNumber != int.MaxValue)
                {
                    maxExecuteOrderNumber = SagaSteps[i].ExecutionSequenceNumber;
                }
                else
                {
                    SagaSteps[i].ExecutionSequenceNumber = maxExecuteOrderNumber++;
                }
            }

            var sageStepsGroups = SagaSteps.GroupBy(i => i.ExecutionSequenceNumber).ToList();

            if (State == SagaStateEnum.Created && SagaSteps.Any())
            {
                var firstGroup = sageStepsGroups[0];
                CurrentExecutionSequenceNumber = firstGroup.First().ExecutionSequenceNumber;
                State = SagaStateEnum.Started;

                foreach (var step in firstGroup)
                {
                    step.State = SagaStepStateEnum.Started;
                }

                return firstGroup.Select(i => i.GetStepEvents(this.Id)).ToList();
            }

            State = SagaStateEnum.Aborted;
            AbortingReason = "Saga State is invalid or there is no steps defined in the Saga.";
            return null;
        }

        public List<SagaEvent>? ProcessEvent(SagaEvent @sagaEvent)
        {
            var currentStep = CurrentSagaStepGroup.FirstOrDefault(i => i.Id == @sagaEvent.StepId);
            if (currentStep == null)
            {
                return null;
            }

            if (@sagaEvent.Succeeded)
            {
                currentStep.State = currentStep.State switch
                {
                    SagaStepStateEnum.Started => SagaStepStateEnum.Succeeded,
                    SagaStepStateEnum.Compensating => SagaStepStateEnum.Compensated,
                    _ => throw new InvalidOperationException()
                };
            }
            else
            {
                if (currentStep.State == SagaStepStateEnum.Started)
                {
                    CancelSubsequentSteps();
                }

                currentStep.State = currentStep.State switch
                {
                    SagaStepStateEnum.Started => SagaStepStateEnum.Failed,
                    SagaStepStateEnum.Compensating => throw new Exception("Compensation operations are not allowed to fail"),
                    _ => throw new InvalidOperationException()
                };
            }

            if (CurrentSagaStepGroup.All(i => i.State > SagaStepStateEnum.Started))
            {
                List<SagaEvent>? @firingEvents = null;

                if (CurrentSagaStepGroup.All(i => i.State == SagaStepStateEnum.Succeeded))
                {
                    @firingEvents = GoNext()?.Select(i => i.GetStepEvents(this.Id)).ToList();
                }
                else if (CurrentSagaStepGroup.All(i => i.State == SagaStepStateEnum.Failed))
                {
                    @firingEvents = GoPrevious()?.Select(i => i.GetStepCompensateEvents(this.Id)).ToList();
                }
                else if (CurrentSagaStepGroup.Any(i => i.State == SagaStepStateEnum.Failed))
                {
                    var needCompensate = CurrentSagaStepGroup.Where(i => i.State != SagaStepStateEnum.Failed && i.HasCompensation == true).ToList();
                    needCompensate.ForEach(i => i.State = SagaStepStateEnum.Compensating);
                    @firingEvents = needCompensate.Select(i => i.GetStepCompensateEvents(this.Id)).ToList();
                }

                UpdateSagaState();

                return @firingEvents ?? null;
            }

            return null;
        }

        #region Private Methods
        private List<SagaStep>? GoNext()
        {
            var next = NextStepGroup;
            if (next == null)
            {
                return null;
            }

            next.ForEach(i => i.State = SagaStepStateEnum.Started);
            CurrentExecutionSequenceNumber = next[0].ExecutionSequenceNumber;
            return next;
        }

        private List<SagaStep>? GoPrevious()
        {
            var prev = PreviousStepGroup;
            while (prev != null)
            {
                CurrentExecutionSequenceNumber = prev[0].ExecutionSequenceNumber;
                if (prev.Any(i => i.HasCompensation))
                {
                    var needCompensate = prev.Where(p => p.HasCompensation).ToList();
                    needCompensate.ForEach(i => i.State = SagaStepStateEnum.Compensating);
                    return needCompensate;
                }

                prev = PreviousStepGroup;
            }

            return null;
        }

        private void CancelSubsequentSteps()
        {
            var currentStepGroupId = SagaStepGroups.FindIndex(x => x.Key == CurrentExecutionSequenceNumber);
            if (currentStepGroupId >= 0)
            {
                for (var idx = currentStepGroupId + 1; idx < SagaStepGroups.Count; idx++)
                {
                    SagaStepGroups[idx].ToList().ForEach(i => i.State = SagaStepStateEnum.Cancelled);
                }
            }
        }

        private void UpdateSagaState()
        {
            if (SagaSteps.All(s => s.State == SagaStepStateEnum.Succeeded))
            {
                State = SagaStateEnum.Completed;
            }
            else if (SagaSteps.All(s =>
                        s.State == SagaStepStateEnum.Started ||
                        s.State == SagaStepStateEnum.Awaiting))
            {
                State = SagaStateEnum.Started;
            }
            else if (SagaSteps.All(s => s.State == SagaStepStateEnum.Failed ||
                                    s.State == SagaStepStateEnum.Compensated ||
                                    s.State == SagaStepStateEnum.Cancelled))
            {
                State = SagaStateEnum.Aborted;
            }
            else
            {
                State = SagaStateEnum.Aborting;
            }
        }

        #endregion Private Methods
    }
}
