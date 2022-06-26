namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class CallBackEventService
    {
        private readonly IBarrierService _barrierService;
        private readonly ILogger<CallBackEventService> _logger;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly IServiceProvider _serviceProvider;

        public CallBackEventService(IServiceProvider serviceProvider)
        {
            _barrierService = serviceProvider.GetService<IBarrierService>();
            _logger = serviceProvider.GetService<ILogger<CallBackEventService>>();
            _distributedLockProvider = serviceProvider.GetRequiredService<IDistributedLockProvider>();
        }

        public async Task<(bool reject, List<SagaEvent> sagaEvents)> ProcessEventAsync(SagaEvent @event)
        {
            using var unitOfWork = _serviceProvider.GetService<IUnitOfWork>();

            var saga = await unitOfWork.SagaRepository.GetSagaByIdAsync(@event.CorrelationId).ConfigureAwait(false);

            if (saga.CurrentSagaStepGroup.Count > 1)
            {
                using var distributedLock = _distributedLockProvider.CreateLock();
                if (!await distributedLock.TryAcquireLockAsync(saga.Id + saga.CurrentExecutionSequenceNumber).ConfigureAwait(false))
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    return (true, null);
                }
            }

            var barrier = _barrierService.CreateBranchBarrier(@event, _logger);
            var result = await barrier.Call<List<SagaEvent>>(unitOfWork.Transaction, async (trans) => await TransitAsync(@event, unitOfWork)).ConfigureAwait(false);
            return (false, result);
        }

        /// <summary>
        /// Transit to new saga step.
        /// if sagaEvent is failed, will start execute compentation step.
        /// </summary>
        /// <param name="sagaEvent">the event of saga step.</param>
        /// <param name="transaction">the transaction from ambient.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<List<SagaEvent>> TransitAsync(SagaEvent sagaEvent, IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug($"Event Name: {sagaEvent.EventName}, {sagaEvent.StepType} - Succeeded: {sagaEvent.Succeeded}");

            var saga = await unitOfWork.SagaRepository.GetSagaByIdAsync(sagaEvent.CorrelationId, cancellationToken).ConfigureAwait(false);
            var nextStepEvents = saga.ProcessEvent(sagaEvent);
            await unitOfWork.SagaRepository.UpdateSagaAsync(saga, cancellationToken).ConfigureAwait(false);

            if (nextStepEvents != null)
            {
                await unitOfWork.EventLogRepository.SaveEventAsync(nextStepEvents, cancellationToken).ConfigureAwait(false);
            }

            return nextStepEvents;
        }
    }
}
