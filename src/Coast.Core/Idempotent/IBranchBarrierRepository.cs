namespace Coast.Core.Idempotent
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading.Tasks;

    public interface IBranchBarrierRepository
    {
        Task<(int affected, string error)> InsertBarrierAsync(IDbConnection conn, TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType, IDbTransaction trans = null);
    }
}
