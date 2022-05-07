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
        private readonly ILogger _logger;

        public BranchBarrier(
            TransactionTypeEnum transactionType,
            long correlationId,
            long stepId,
            TransactionStepTypeEnum stepType,
            IBranchBarrierRepository branchBarrierRepository,
            ILogger logger)
        {
            TransactionType = transactionType;
            CorrelationId = correlationId;
            StepId = stepId;
            StepType = stepType;
            _branchBarrierRepository = branchBarrierRepository;
            _logger = logger;
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

        public async Task Call(IDbConnection db, Func<IDbTransaction, Task> busiCall)
        {
            // https://zhuanlan.zhihu.com/p/388444465
            if (db.State != ConnectionState.Open)
            {
                db.Open();
            }

            var tx = db.BeginTransaction();
            try
            {
                (int affected1, string error1) = await _branchBarrierRepository.InsertBarrierAsync(db,
                                                                                                   TransactionType,
                                                                                                   CorrelationId,
                                                                                                   StepId,
                                                                                                   StepType,
                                                                                                   tx);

                int affected2 = 0;
                string error2 = string.Empty;
                if (StepType == TransactionStepTypeEnum.Compensate)
                {
                    (affected2,  error2) = await _branchBarrierRepository.InsertBarrierAsync(db,
                                                                                             TransactionType,
                                                                                             CorrelationId,
                                                                                             StepId,
                                                                                             TransactionStepTypeEnum.Commit,
                                                                                             tx);
                }

                if (affected1 != 0 && affected2 == 0 && string.IsNullOrWhiteSpace(error1) && string.IsNullOrWhiteSpace(error2))
                {
                    await busiCall(tx);
                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                tx.Rollback();
                _logger.LogError(ex, $"Call error, CorrelationId={CorrelationId}, TransactionType={TransactionType}, StepId={StepId}, StepType={StepType}");

                throw;
            }
        }
    }
}