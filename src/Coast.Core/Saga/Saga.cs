namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
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
        public Saga(IEnumerable<ISagaRequest> steps)
        {
            SagaSteps.AddRange(steps.Select(i => new SagaStep(Id, i)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Saga"/> class.
        /// </summary>
        public Saga()
        {
        }

        public long Id { get; private set; } = SnowflakeId.Default().NextId();

        public SagaStateEnum State { get; private set; } = SagaStateEnum.Created;

        public DateTime CreateTime { get; private set; }

        public int CurrenExecuteOrder { get; private set; }

        public string? AbortingReason { get; set; }

        private IEnumerable<SagaStep> CurrentSagaStepGroup => SagaSteps.Where(s => s.ExecuteOrder == CurrenExecuteOrder);

        private List<IGrouping<int, SagaStep>> SagaStepGroups => SagaSteps.GroupBy(i => i.ExecuteOrder).OrderBy(i => i.Key).ToList();

        private List<SagaStep>? NextStepGroup
        {
            get
            {
                var idx = SagaStepGroups.FindIndex(s => s.Key == CurrenExecuteOrder);

                if (idx == -1)
                {
                    return null;
                }

                Interlocked.Increment(ref idx);
                if (idx == SagaSteps.Count)
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
                var idx = SagaStepGroups.FindIndex(s => s.Key == CurrenExecuteOrder);

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

        public Saga AddStep(ISagaRequest sagaRequest, bool hasCompensation = default, int executeOrder = int.MaxValue)
        {
            SagaSteps.Add(new SagaStep(Id, sagaRequest, hasCompensation, executeOrder));
            return this;
        }

        public Saga AddStep(string stepEventName, object sagaRequest, bool hasCompensation = default, int executeOrder = int.MaxValue)
        {
            SagaSteps.Add(new SagaStep(Id, stepEventName, sagaRequest, hasCompensation, executeOrder));
            return this;
        }

        public List<SagaEvent> Start()
        {
            if (SagaSteps.Count == 0)
            {
                State = SagaStateEnum.Completed;
                return null;
            }

            SagaSteps = SagaSteps.OrderBy(i => i.ExecuteOrder).ToList();
            int maxExecuteOrderNumber = default;

            for (int i = 0; i < SagaSteps.Count; i++)
            {
                if (SagaSteps[i].ExecuteOrder != int.MaxValue)
                {
                    maxExecuteOrderNumber = SagaSteps[i].ExecuteOrder;
                }
                else
                {
                    SagaSteps[i].ExecuteOrder = maxExecuteOrderNumber++;
                }
            }

            var sageStepsGroups = SagaSteps.GroupBy(i => i.ExecuteOrder).ToList();

            if (State == SagaStateEnum.Created && SagaSteps.Any())
            {
                var firstGroup = sageStepsGroups[0];
                CurrenExecuteOrder = firstGroup.First().ExecuteOrder;
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
            var currentStep = CurrentSagaStepGroup.FirstOrDefault(i => i.Id == @sagaEvent.SagaStepId);
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
                    SagaStepStateEnum.Compensating => SagaStepStateEnum.Failed,
                    _ => throw new InvalidOperationException()
                };
            }

            if (CurrentSagaStepGroup.All(i => i.State > SagaStepStateEnum.Started))
            {
                List<SagaEvent>? @firingEvents = null;
                switch (currentStep.State)
                {
                    case SagaStepStateEnum.Succeeded:
                        @firingEvents = GoNext()?.Select(i => i.GetStepEvents(this.Id)).ToList();
                        break;
                    case SagaStepStateEnum.Compensated:
                    case SagaStepStateEnum.Failed:
                        @firingEvents = GoPrevious()?.Select(i => i.GetStepCompensateEvents(this.Id)).ToList();
                        break;
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
            CurrenExecuteOrder = next[0].ExecuteOrder;
            return next;
        }

        private List<SagaStep>? GoPrevious()
        {
            var prev = PreviousStepGroup;
            while (prev != null)
            {
                CurrenExecuteOrder = prev[0].ExecuteOrder;
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
            var currentStepIdx = SagaStepGroups.FindIndex(x => x.Key == CurrenExecuteOrder);
            if (currentStepIdx >= 0)
            {
                for (var idx = currentStepIdx + 1; idx < SagaStepGroups.Count; idx++)
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
