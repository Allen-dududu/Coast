﻿namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;

    public class SagaManager : ISagaManager
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEventBus _eventPublisher;
        private readonly ILogger<SagaManager> _logger;

        public SagaManager(IUnitOfWork repositoryFactory, IEventBus eventPublisher, ILogger<SagaManager> logger)
        {
            _unitOfWork = repositoryFactory;
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
            await _unitOfWork.SagaRepository.SaveSagaAsync(saga, cancellationToken).ConfigureAwait(false);

            if (@sagaEvents != null)
            {
                await _unitOfWork.EventLogRepository.SaveEventAsync(@sagaEvents, cancellationToken).ConfigureAwait(false);
            }

            _unitOfWork.Commit();

            if (@sagaEvents != null)
            {
                foreach (var @event in @sagaEvents)
                {
                    await _eventPublisher.PublishWithLogAsync(@event).ConfigureAwait(false);
                }
            }
        }
    }
}
