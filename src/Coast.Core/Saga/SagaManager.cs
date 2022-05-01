namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;

    public class SagaManager
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IEventBus _eventPublisher;
        private readonly ILogger<SagaManager> _logger;

        public SagaManager(IRepositoryFactory repositoryFactory, IEventBus eventPublisher, ILogger<SagaManager> logger)
        {
            _repositoryFactory = repositoryFactory;
            _eventPublisher = eventPublisher;
            _logger = logger;
        }

        /// <summary>
        /// Create a saga that executes the saga steps in sequence.
        /// </summary>
        /// <param name="steps">saga steps.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task{TResult}"/> saga instance.</returns>
        public async Task<Saga> CreateAsync(IEnumerable<ISagaRequest> steps = default, CancellationToken cancellationToken = default)
        {
            var saga = new Saga(steps);
            using var session = _repositoryFactory.OpenSession();
            var sagaRepository = session.ConstructSagaRepository();
            await sagaRepository.AddSagaAsync(saga, cancellationToken);
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

            if (saga.State != SagaStateEnum.Created)
            {
                throw new ArgumentException("The saga has been created before.");
            }

            var sagaEvents = saga.Start();

            using var session = _repositoryFactory.OpenSession();
            session.StartTransaction();

            var sagaRepository = session.ConstructSagaRepository();
            var eventLogRepository = session.ConstructEventLogRepository();

            try
            {
                await sagaRepository.UpdateSagaAsync(saga, cancellationToken);
                if (sagaEvents != null)
                {
                    await eventLogRepository.SaveEventAsync(sagaEvents, cancellationToken);
                }

                session.CommitTransaction();
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                _logger.LogError(ex, $"Failed to save the saga {saga}");
                throw;
            }

            if (sagaEvents != null)
            {
                foreach (var @event in sagaEvents)
                {
                    await eventLogRepository.MarkEventAsInProgressAsync(@event.Id);
                    _eventPublisher.Publish(@event, cancellationToken);
                    await eventLogRepository.MarkEventAsPublishedAsync(@event.Id);
                }
            }
        }

        /// <summary>
        /// Transit to new saga step.
        /// if sagaEvent is failed, will start execute compentation step.
        /// </summary>
        /// <param name="sagaEvent">the event of saga step.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TransitAsync(SagaEvent sagaEvent, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{sagaEvent.EventType} - Succeeded: {sagaEvent.Succeeded}");

            using var session = _repositoryFactory.OpenSession();
            session.StartTransaction();
            var sagaRepository = session.ConstructSagaRepository();
            var eventLogRepository = session.ConstructEventLogRepository();

            var saga = await sagaRepository.GetSagaByIdAsync(sagaEvent.CorrelationId, cancellationToken);
            var nextStepEvents = saga.ProcessEvent(sagaEvent);

            try
            {
                await sagaRepository.UpdateSagaAsync(saga, cancellationToken);

                if (nextStepEvents != null)
                {
                    await eventLogRepository.SaveEventAsync(nextStepEvents, cancellationToken);
                }

                session.CommitTransaction();
            }
            catch (Exception ex)
            {
                session.RollbackTransaction();
                _logger.LogError(ex, $"Failed to save the saga {saga}");
                throw;
            }

            if (nextStepEvents != null)
            {
                foreach (var @event in nextStepEvents)
                {
                    await eventLogRepository.MarkEventAsInProgressAsync(@event.Id);
                    _eventPublisher.Publish(@event, cancellationToken);
                    await eventLogRepository.MarkEventAsPublishedAsync(@event.Id);
                }
            }
        }
    }
}
