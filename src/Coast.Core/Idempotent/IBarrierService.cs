namespace Coast.Core
{
    using Microsoft.Extensions.Logging;

    public interface IBarrierService
    {
        BranchBarrier CreateBranchBarrier(TransactionTypeEnum transactionType, long correlationId, long sagaStepId, TransactionStepTypeEnum eventType, ILogger? logger = null);

        BranchBarrier CreateBranchBarrier(SagaEvent @event, ILogger? logger = null);

    }
}
