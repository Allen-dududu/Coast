namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.Idempotent;
    using Microsoft.Extensions.Logging;

    public class DefaultBarrierService : IBarrierService
    {
        private readonly ILogger<DefaultBarrierService> _logger;
        private readonly IBranchBarrierRepository _branchBarrierRepository;

        public DefaultBarrierService(ILogger<DefaultBarrierService> logger, IBranchBarrierRepository branchBarrierRepository)
        {
            _logger = logger;
            _branchBarrierRepository = branchBarrierRepository;
        }

        public BranchBarrier CreateBranchBarrier(TransactionTypeEnum transactionType, long correlationId, long sagaStepId, TransactionStepTypeEnum eventType, ILogger? logger = null)
        {
            if (logger is null)
            {
                logger = _logger;
            }

            var bb = new BranchBarrier(transactionType, correlationId, sagaStepId, eventType, _branchBarrierRepository, logger);
            return bb;
        }

        public BranchBarrier CreateBranchBarrier(SagaEvent @event, ILogger logger = null)
        {
            if (@event is null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            if (logger is null)
            {
                logger = _logger;
            }

            var bb = new BranchBarrier(@event.TransactionType, @event.CorrelationId, @event.SagaStepId, @event.EventType, _branchBarrierRepository, logger);
            return bb;
        }
    }
}
