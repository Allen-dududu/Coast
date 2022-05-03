namespace Coast.Core
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core.Idempotent;
    using Microsoft.Extensions.Logging;

    public class BranchBarrier
    {
        private readonly IBranchBarrierRepository _branchBarrierRepository;

        public BranchBarrier(TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType, IBranchBarrierRepository branchBarrierRepository, ILogger logger)
        {
            TransactionType = transactionType;
            CorrelationId = correlationId;
            StepId = stepId;
            StepType = stepType;
            _branchBarrierRepository = branchBarrierRepository;
        }

        public int Id { get; set; }

        /// <summary>
        /// Gets or sets transaction Type.
        /// </summary>
        public TransactionTypeEnum TransactionType { get; set; }

        /// <summary>
        /// Gets or sets global transaction id.
        /// maybe Saga or TCC or others.
        /// </summary>
        public long CorrelationId { get; set; }

        public long StepId { get; set; }

        public TransactionStepTypeEnum StepType { get; set; }

        public async Task Call(IDbConnection db, Func<IDbConnection, Task> busiCall)
        {

        }
    }
}