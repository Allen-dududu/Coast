namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.Idempotent;
    using Dapper;
    using Microsoft.Extensions.Options;

    public class BranchBarrierRepository : IBranchBarrierRepository
    {
        private readonly string _tableName;

        public BranchBarrierRepository(IOptions<DBOptions> options)
        {
            _tableName = $"\"{options.Value.Schema}\".\"Barrier\"";
        }

        public async Task<(int affected, string error)> InsertBarrierAsync(IDbConnection conn, TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType, IDbTransaction trans = null)
        {
            var InsertIgnoreSql =
 $@"INSERT INTO {_tableName} (""Id"", ""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"", ""CreationTime"")
VALUES(@Id, @TransactionType, @CorrelationId, @StepId, @StepType, @CreationTime) 
ON CONFLICT (""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"") 
DO NOTHING;";

            int affected = 0;
            string error = string.Empty;
            try
            {
                affected = await conn.ExecuteAsync(
                    InsertIgnoreSql,
                    new { id = SnowflakeId.Default().NextId(), TransactionType = transactionType, CorrelationId = correlationId, StepId = stepId, StepType = stepType, CreationTime = DateTime.UtcNow },
                    transaction: trans).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return (affected, error);
        }
    }
}
