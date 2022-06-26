namespace Coast.Core
{
    using System;
    using System.Data;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class DefaultBarrierService : IBarrierService
    {
        private readonly ILogger<DefaultBarrierService> _logger;
        private readonly IBranchBarrierRepository _branchBarrierRepository;
        private readonly Func<IDbTransaction, IEventLogRepository> _eventLogRepositoryFactory;

        public DefaultBarrierService(ILogger<DefaultBarrierService> logger, IBranchBarrierRepository branchBarrierRepository, Func<IDbTransaction, IEventLogRepository> eventLogRepositoryFactory)
        {
            _logger = logger;
            _branchBarrierRepository = branchBarrierRepository;
            _eventLogRepositoryFactory = eventLogRepositoryFactory;
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

            var bb = new BranchBarrier(@event, _branchBarrierRepository, logger, _eventLogRepositoryFactory);
            return bb;
        }
    }
}
