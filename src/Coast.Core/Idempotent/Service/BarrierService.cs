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

        public BranchBarrier CreateBranchBarrier(string transType, string gid, string branchID, string op, ILogger? logger = null)
        {
            throw new NotImplementedException();
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
