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
 @"INSERT INTO Coast_Barrier (""Id"", ""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"", ""CreationTime"")
VALUES(@Id, @TransactionType, @CorrelationId, @StepId, @StepType, @CreationTime) 
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
                    new { id = SnowflakeId.Default().NextId(),  TransactionType = transactionType, CorrelationId = correlationId, StepId = stepId, StepType = stepType, CreationTime = DateTime.UtcNow },
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
