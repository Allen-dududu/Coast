namespace Coast.Core
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class BranchBarrier
    {
        private readonly IBranchBarrierRepository _branchBarrierRepository;
        private readonly ILogger _logger;
        private readonly Func<IDbTransaction, IEventLogRepository> _eventLogRepositoryFactory;

        public BranchBarrier(
            TransactionTypeEnum transactionType,
            long correlationId,
            long stepId,
            TransactionStepTypeEnum stepType,
            IBranchBarrierRepository branchBarrierRepository,
            ILogger logger,
            Func<IDbTransaction, IEventLogRepository> eventLogRepositoryFactory)
        {
            TransactionType = transactionType;
            CorrelationId = correlationId;
            StepId = stepId;
            StepType = stepType;
            _branchBarrierRepository = branchBarrierRepository;
            _logger = logger;
            _eventLogRepositoryFactory = eventLogRepositoryFactory;
        }

        public BranchBarrier(
            SagaEvent @event,
            IBranchBarrierRepository branchBarrierRepository,
            ILogger logger,
            Func<IDbTransaction, IEventLogRepository> eventLogRepositoryFactory)
        {
            _sagaEvent = @event;
            TransactionType = @event.TransactionType;
            CorrelationId = @event.CorrelationId;
            StepId = @event.StepId;
            StepType = @event.StepType;
            _branchBarrierRepository = branchBarrierRepository;
            _logger = logger;
            _eventLogRepositoryFactory = eventLogRepositoryFactory;
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

        public async Task Call(IDbTransaction trans, Func<IDbTransaction, Task> busiCall)
        {
            // https://zhuanlan.zhihu.com/p/388444465
            try
            {
                (int affected1, int affected2) = await IdempotentCheck(trans);

                if (affected1 != 0 && affected2 == 0)
                {
                    await busiCall(trans).ConfigureAwait(false);
                    await SaveCallBackEventLog(trans).ConfigureAwait(false);
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

        public async Task<T> Call<T>(IDbTransaction trans, Func<IDbTransaction, Task<T>> busiCall)
        {
            // https://zhuanlan.zhihu.com/p/388444465
            try
            {
                (int affected1, int affected2) = await IdempotentCheck(trans);

                if (affected1 != 0 && affected2 == 0)
                {
                    await SaveCallBackEventLog(trans).ConfigureAwait(false);
                    var result = await busiCall(trans).ConfigureAwait(false);
                    trans.Commit();

                    return result;
                }

                _logger.LogDebug($"The event has been consumed. CorrelationId={CorrelationId}, TransactionType={TransactionType}, StepId={StepId}, StepType={StepType}");
                return default(T);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, $"Call error, CorrelationId={CorrelationId}, TransactionType={TransactionType}, StepId={StepId}, StepType={StepType}");

                throw;
            }
        }

        private async Task<(int affected1, int affected2)> IdempotentCheck(IDbTransaction trans)
        {
            (int affected1, string error1) = await _branchBarrierRepository.InsertBarrierAsync(trans,
                                                                                   TransactionType,
                                                                                   CorrelationId,
                                                                                   StepId,
                                                                                   StepType).ConfigureAwait(false);

            int affected2 = 0;
            string error2 = string.Empty;
            if (StepType == TransactionStepTypeEnum.Compensate)
            {
                (affected2, error2) = await _branchBarrierRepository.InsertBarrierAsync(trans,
                                                                                         TransactionType,
                                                                                         CorrelationId,
                                                                                         StepId,
                                                                                         TransactionStepTypeEnum.Commit).ConfigureAwait(false);
            }

            if (!string.IsNullOrWhiteSpace(error1) || !string.IsNullOrWhiteSpace(error2))
            {
                throw new Exception($"Insert Barrier Error: error1 = {error1}, error2 = {error2}");
            }

            return (affected1, affected2);
        }

        private async Task SaveCallBackEventLog(IDbTransaction tx)
        {
            var eventLogRepo = _eventLogRepositoryFactory(tx);
            await eventLogRepo.SaveEventAsync(_sagaEvent).ConfigureAwait(false);
        }
    }
}