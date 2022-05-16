namespace Coast.Core
{
    using Coast.Core.DataLayer;
    using Coast.Core.Idempotent;
    using Microsoft.Extensions.Logging;
    using System;

    public class DefaultBarrierService : IBarrierService
    {
        private readonly ILogger<DefaultBarrierService> _logger;
        private readonly IBranchBarrierRepository _branchBarrierRepository;
        private readonly IRepositoryFactory _repositoryFactor;

        public DefaultBarrierService(ILogger<DefaultBarrierService> logger, IBranchBarrierRepository branchBarrierRepository, IRepositoryFactory repositoryFactor)
        {
            _logger = logger;
            _branchBarrierRepository = branchBarrierRepository;
            _repositoryFactor = repositoryFactor;
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

            var bb = new BranchBarrier(@event, _branchBarrierRepository, logger, _repositoryFactor);
            return bb;
        }
    }
}
