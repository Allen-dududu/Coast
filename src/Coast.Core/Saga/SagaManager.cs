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
            return saga;
        }

        /// <summary>
        /// Create a saga that executes the saga steps in sequence.
        /// </summary>
        /// <param name="steps">saga steps.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> saga instance.</returns>
        public async Task<Saga> CreateAsync(IEnumerable<SagaStep> steps = default, CancellationToken cancellationToken = default)
        {
            var saga = new Saga(steps);
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

            if (saga.Status != SagaStatusEnum.Creating)
            {
                throw new ArgumentException("The saga has been created before.");
            }

            await _sagaRepository.AddSagaAsync(saga, cancellationToken);

            var sagaEvent = saga.Start();
            if (sagaEvent != null)
            {
                _eventPublisher.Publish(sagaEvent);
            }

            await _sagaRepository.UpdateSagaByIdAsync(saga, cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sagaEvent"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task TransitAsync(SagaEvent sagaEvent, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"{sagaEvent.EventType} - Succeeded: {sagaEvent.Succeeded}");
            var saga = await _dao.GetByIdAsync<Saga>(sagaEvent.SagaId, cancellationToken);
            var nextStepEvent = saga.ProcessEvent(sagaEvent);
            if (nextStepEvent != null)
            {
                await _eventPublisher.PublishAsync(nextStepEvent, nextStepEvent.ServiceName, cancellationToken);
            }

            await _dao.UpdateByIdAsync(saga.Id, saga);
        }
    }
}
