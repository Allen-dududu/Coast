﻿namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core;
    using Dapper;
    using Microsoft.Extensions.Options;

    public class BranchBarrierRepository : IBranchBarrierRepository
    {
        private readonly string _tableName;

        public BranchBarrierRepository(IOptions<DBOptions> options)
        {
            _tableName = $"\"{options.Value.Schema}\".\"Barrier\"";
        }

        public async Task<(int affected, string error)> InsertBarrierAsync(IDbTransaction trans, TransactionTypeEnum transactionType, long correlationId, long stepId, TransactionStepTypeEnum stepType, bool isCallBack)
        {
            var InsertIgnoreSql =
 $@"INSERT INTO {_tableName} (""Id"", ""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"", ""CreationTime"", ""IsCallBack"")
VALUES(@Id, @TransactionType, @CorrelationId, @StepId, @StepType, @CreationTime, @IsCallBack) 
ON CONFLICT (""TransactionType"", ""CorrelationId"", ""StepId"",""StepType"", ""IsCallBack"") 
DO NOTHING;";

            int affected = 0;
            string error = string.Empty;
            try
            {
                affected = await trans.Connection.ExecuteAsync(
                    InsertIgnoreSql,
                    new { id = SnowflakeId.Default().NextId(), TransactionType = transactionType, CorrelationId = correlationId, StepId = stepId, StepType = stepType, CreationTime = DateTime.UtcNow, IsCallBack = isCallBack },
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
