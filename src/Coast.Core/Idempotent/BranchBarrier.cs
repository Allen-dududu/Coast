namespace Coast.Core
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus.EventLog;
    using Coast.Core.Idempotent;
    using Microsoft.Extensions.Logging;

    public class BranchBarrier
    {
        private readonly IBranchBarrierRepository _branchBarrierRepository;
        private readonly ILogger _logger;
        private readonly IRepositoryFactory _repositoryFactory;

        public BranchBarrier(
            TransactionTypeEnum transactionType,
            long correlationId,
            long stepId,
            TransactionStepTypeEnum stepType,
            IBranchBarrierRepository branchBarrierRepository,
            ILogger logger,
            IRepositoryFactory repositoryFactory)
        {
            TransactionType = transactionType;
            CorrelationId = correlationId;
            StepId = stepId;
            StepType = stepType;
            _branchBarrierRepository = branchBarrierRepository;
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        public BranchBarrier(
            SagaEvent @event,
            IBranchBarrierRepository branchBarrierRepository,
            ILogger logger,
            IRepositoryFactory repositoryFactory)
        {
            _sagaEvent = @event;
            _branchBarrierRepository = branchBarrierRepository;
            _logger = logger;
            _repositoryFactory = repositoryFactory;
        }

        private SagaEvent _sagaEvent { get; set; }
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

        public async Task Call(IDbConnection conn, Func<IDbConnection, IDbTransaction, Task> busiCall)
        {
            // https://zhuanlan.zhihu.com/p/388444465
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }

            var trans = conn.BeginTransaction();
            try
            {
                (int affected1, string error1) = await _branchBarrierRepository.InsertBarrierAsync(conn,
                                                                                                   _sagaEvent.TransactionType,
                                                                                                   _sagaEvent.CorrelationId,
                                                                                                   _sagaEvent.StepId,
                                                                                                   _sagaEvent.EventType,
                                                                                                   trans);

                int affected2 = 0;
                string error2 = string.Empty;
                if (StepType == TransactionStepTypeEnum.Compensate)
                {
                    (affected2,  error2) = await _branchBarrierRepository.InsertBarrierAsync(conn,
                                                                                             TransactionType,
                                                                                             CorrelationId,
                                                                                             StepId,
                                                                                             TransactionStepTypeEnum.Commit,
                                                                                             trans);
                }

                if (affected1 != 0 && affected2 == 0 && string.IsNullOrWhiteSpace(error1) && string.IsNullOrWhiteSpace(error2))
                {
                    await busiCall(conn, trans);
                    await SaveCallBackEventLog(conn, trans);
                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, $"Call error, CorrelationId={CorrelationId}, TransactionType={TransactionType}, StepId={StepId}, StepType={StepType}");

                throw;
            }
        }

        private async Task SaveCallBackEventLog(IDbConnection db, IDbTransaction tx)
        {
            var session = _repositoryFactory.OpenSession(db);
            session.StartTransaction(tx);
            var eventLogRepository = session.ConstructEventLogRepository();
            await eventLogRepository.SaveEventAsync(_sagaEvent);
        }
    }
}