﻿namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.DataLayer;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class CallBackEventService
    {
        private readonly IBarrierService _barrierService;
        private readonly ILogger<CallBackEventService> _logger;
        private readonly IConnectionProvider _connectionProvider;
        private readonly IRepositoryFactory _repositoryFactory;

        public CallBackEventService(IServiceProvider serviceProvider)
        {
            _repositoryFactory = serviceProvider.GetService<IRepositoryFactory>();
            _barrierService = serviceProvider.GetService<IBarrierService>();
            _connectionProvider = serviceProvider.GetService<IConnectionProvider>();
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

            return nextStepEvents;
        }
    }
}