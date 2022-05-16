namespace Coast.Core
{
    using Coast.Core.DataLayer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public class CallBackEventService
    {
        // Instantiate a Singleton of the Semaphore with a value of 1. This means that only 1 thread can be granted access at a time.
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly IBarrierService _barrierService;
        private readonly ILogger<CallBackEventService> _logger;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IRepositoryFactory _repositoryFactory;

        public CallBackEventService(IServiceProvider serviceProvider)
        {
            _repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            _barrierService = serviceProvider.GetService<IBarrierService>();
            _connectionProvider = serviceProvider.GetService<IConnectionProvider>();
            _logger = serviceProvider.GetService<ILogger<CallBackEventService>>();
        }

        public async Task<List<SagaEvent>> ProcessEventAsync(SagaEvent @event)
        {

            using var connection = _connectionProvider.OpenConnection();
            var barrier = _barrierService.CreateBranchBarrier(@event, _logger);
            var result = await barrier.Call<List<SagaEvent>>(connection, async (connection, trans) => await TransitAsync(@event, connection, trans));
            return result;
        }

        /// <summary>
        /// Transit to new saga step.
        /// if sagaEvent is failed, will start execute compentation step.
        /// </summary>
        /// <param name="sagaEvent">the event of saga step.</param>
        /// <param name="transaction">the transaction from ambient.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<SagaEvent>> TransitAsync(SagaEvent sagaEvent, IDbConnection conn, IDbTransaction transaction, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Event Name: {sagaEvent.EventName}, {sagaEvent.StepType} - Succeeded: {sagaEvent.Succeeded}");

            await _semaphoreSlim.WaitAsync();

            try
            {
                // should not close connection
                var session = _repositoryFactory.OpenSession(conn);

                // transction commit by Barrier service.
                session.StartTransaction(transaction);
                var sagaRepository = session.ConstructSagaRepository();
                var eventLogRepository = session.ConstructEventLogRepository();

                var saga = await sagaRepository.GetSagaByIdAsync(sagaEvent.CorrelationId, cancellationToken);
                var nextStepEvents = saga.ProcessEvent(sagaEvent);

                await sagaRepository.UpdateSagaAsync(saga, cancellationToken);

                if (nextStepEvents != null)
                {
                    await eventLogRepository.SaveEventAsync(nextStepEvents, cancellationToken);
                }

                return nextStepEvents;
            }
            finally
            {
                // When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                // This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                _semaphoreSlim.Release();
            }

        }
    }
}
