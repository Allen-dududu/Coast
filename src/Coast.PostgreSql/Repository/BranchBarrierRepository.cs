namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.Idempotent;
    using Dapper;

    public class BranchBarrierRepository : IBranchBarrierRepository
    {
        private const string InsertIgnoreSql =
 @"INSERT INTO Coast_Barrier (""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"")
VALUES(@TransactionType, @CorrelationId, @StepId, @StepType) 
ON CONFLICT ""Barrier_Id"" 
DO NOTHING;";

        public async Task<(int affected, string error)> InsertBarrierAsync(IDbConnection db, TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType, IDbTransaction tx = null)
        {
            int affected = 0;
            string error = string.Empty;
            try
            {
                affected = await db.ExecuteAsync(
                    InsertIgnoreSql,
                    new { TransactionType = transactionType, CorrelationId = correlationId, StepId = stepId, StepType = stepType },
                    transaction: tx).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return (affected, error);
        }
    }
}
