namespace Coast.Core
{
    using System.Data;
    using System.Threading.Tasks;

    public interface IBranchBarrierRepository
    {
        Task<(int affected, string error)> InsertBarrierAsync(IDbTransaction trans, TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType);
    }
}
