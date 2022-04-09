namespace Coast.Core.Saga
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;

    public class SagaManager
    {
        private readonly ISagaRepository _sagaRepository;
        private readonly IEventBus _eventPublisher;

        public SagaManager(ISagaRepository sagaRepository, IEventBus eventPublisher)
        {
            _sagaRepository = sagaRepository;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Create a saga that executes the saga steps in sequence.
        /// </summary>
        /// <param name="steps">saga steps.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> saga instance.</returns>
        public async Task<Saga> CreateAsync(IEnumerable<ISagaRequestBody> steps = default, CancellationToken cancellationToken = default)
        {
            var saga = new Saga(steps);
            await _sagaRepository.AddSagaAsync(saga, cancellationToken);
            return saga;
        }

        /// <summary>
        /// start a new saga what executes the saga steps in sequence. And execute the first saga step.
        /// </summary>
        /// <param name="saga">saga steps.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            if (saga == null)
            {
                throw new ArgumentNullException(nameof(saga));
            }

            if (saga.Status != SagaStatusEnum.Created)
            {
                throw new ArgumentException("The saga has been created before.");
            }

            var sagaEvents = saga.Start();
            await _sagaRepository.UpdateSagaByIdAsync(saga, cancellationToken);

            if (sagaEvents != null)
            {
                sagaEvents.ForEach(i => _eventPublisher.Publish(i, cancellationToken));
            }
        }

        /// <summary>
        /// Transit to new saga step.
        /// if sagaEvent is failed, will start excute compentation step.
        /// </summary>
        /// <param name="sagaEvent">the event of saga step.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TransitAsync(SagaEvent sagaEvent, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{sagaEvent.EventType} - Succeeded: {sagaEvent.Succeeded}");
            var saga = await _sagaRepository.GetSagaByIdAsync(sagaEvent.CorrelationId, cancellationToken);
            await _sagaRepository.UpdateSagaByIdAsync(saga);
            var nextStepEvent = saga.ProcessEvent(sagaEvent);

            if (nextStepEvent != null)
            {
                nextStepEvent.ForEach(i => _eventPublisher.Publish(i, cancellationToken));
            }
        }
    }
}
