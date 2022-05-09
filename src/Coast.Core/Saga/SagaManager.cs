namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;

    public class SagaManager : ISagaManager
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
        public async Task<Saga> CreateAsync(IEnumerable<EventRequestBody> steps = default, CancellationToken cancellationToken = default)
        {
            var saga = new Saga(steps);
            using var session = _repositoryFactory.OpenSession();
            var sagaRepository = session.ConstructSagaRepository();
            await sagaRepository.SaveSagaAsync(saga, cancellationToken);
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
            if (saga is null)
            {
                throw new ArgumentNullException(nameof(saga));
            }

            if (saga.State != SagaStateEnum.Created)
            {
                throw new ArgumentException("The saga has been created before.");
            }

            var @sagaEvents = saga.Start();

            using var session = _repositoryFactory.OpenSession();
            using var transaction = session.StartTransaction();

            var sagaRepository = session.ConstructSagaRepository();
            var eventLogRepository = session.ConstructEventLogRepository();

            try
            {
                await sagaRepository.SaveSagaStepsAsync(saga, cancellationToken);
                await sagaRepository.UpdateSagaAsync(saga, cancellationToken);
                if (@sagaEvents != null)
                {
                    await eventLogRepository.SaveEventAsync(@sagaEvents, cancellationToken);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, $"Failed to save the saga {saga}");
                throw;
            }

            if (@sagaEvents != null)
            {
                foreach (var @event in @sagaEvents)
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
        /// <param name="transaction">the transaction from ambient.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task TransitAsync(SagaEvent sagaEvent, IDbConnection conn, IDbTransaction transaction, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation($"{sagaEvent.EventType} - Succeeded: {sagaEvent.Succeeded}");

            // should not close connection
            var session = _repositoryFactory.OpenSession(conn);

            // transction commit by Barrier service.
            session.StartTransaction(transaction);
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save the saga {saga}");
                throw;
            }

            if (nextStepEvents != null)
            {
                using var session2 = _repositoryFactory.OpenSession();
                var eventLogRepository2 = session.ConstructEventLogRepository();

                foreach (var @event in nextStepEvents)
                {
                    await eventLogRepository2.MarkEventAsInProgressAsync(@event.Id);
                    _eventPublisher.Publish(@event, cancellationToken);
                    await eventLogRepository2.MarkEventAsPublishedAsync(@event.Id);
                }
            }
        }
    }
}
