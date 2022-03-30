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
        public async Task<Saga> CreateAsync(IEnumerable<(SagaStep, SagaStep?)> steps, CancellationToken cancellationToken = default)
        {
            var saga = new Saga(steps);
            await _sagaRepository.AddSagaAsync(saga, cancellationToken);
            return saga;
        }

        /// <summary>
        /// start a saga what executes the saga steps in sequence. And execute the first saga step.
        /// </summary>
        /// <param name="saga">saga steps.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task StartAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            var sagaEvent = saga.Start();
            if (sagaEvent != null)
            {
                _eventPublisher.Publish(sagaEvent);
            }

            await _sagaRepository.UpdateSagaByIdAsync(saga, cancellationToken);
        }
    }
}
