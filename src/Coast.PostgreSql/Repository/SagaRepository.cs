namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Dapper;

    internal class SagaRepository : RepositoryBase, ISagaRepository
    {
        private readonly string _sagaTableName;
        private readonly string _sagaStepTableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaRepository"/> class.
        /// </summary>
        public SagaRepository(string schemaName, IDbTransaction transaction) : base(transaction)
        {
            _sagaTableName = $"\"{schemaName}\".\"Saga\"";
            _sagaStepTableName = $"\"{schemaName}\".\"SagaStep\"";
        }

        /// <inheritdoc/>
        public async Task SaveSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string InsertSagaSql =
$@"INSERT INTO {_sagaTableName} 
(""Id"", ""State"", ""CreationTime"", ""CurrentExecutionSequenceNumber"") 
VALUES (@Id, @State, @CreationTime, @CurrentExecutionSequenceNumber); ";
            string InsertSagaStepSql =
$@"INSERT INTO {_sagaStepTableName}
(""Id"", ""CorrelationId"", ""EventName"", ""HasCompensation"", ""State"", ""RequestBody"", ""CreationTime"", ""FailedReason"", ""ExecutionSequenceNumber"") 
VALUES (@Id, @CorrelationId, @EventName, @HasCompensation, @State, @RequestBody, @CreationTime,@FailedReason, @ExecutionSequenceNumber); ";

            await Connection.ExecuteAsync(
                    InsertSagaSql,
                    new { Id = saga.Id, State = SagaStateEnum.Started, CreationTime = DateTime.UtcNow, CurrentExecutionSequenceNumber = saga.CurrentExecutionSequenceNumber },
                    transaction: Transaction).ConfigureAwait(false);

            foreach (var step in saga.SagaSteps)
            {
                await Connection.ExecuteAsync(
                    InsertSagaStepSql,
                    new
                    {
                        Id = step.Id,
                        CorrelationId = saga.Id,
                        EventName = step.EventName,
                        State = step.State,
                        RequestBody = step.RequestBody,
                        CreationTime = DateTime.UtcNow,
                        HasCompensation = step.HasCompensation,
                        FailedReason = step.FailedReason,
                        ExecutionSequenceNumber = step.ExecutionSequenceNumber,
                    },
                    transaction: Transaction).ConfigureAwait(false);
            }
        }

        public async Task SaveSagaStepsAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string InsertSagaStepSql =
$@"INSERT INTO {_sagaStepTableName}
(""Id"", ""CorrelationId"", ""EventName"", ""HasCompensation"", ""State"", ""RequestBody"", ""CreationTime"", ""FailedReason"", ""ExecutionSequenceNumber"") 
VALUES (@Id, @CorrelationId, @EventName, @HasCompensation, @State, @RequestBody, @CreationTime,@FailedReason, @ExecutionSequenceNumber); ";

            foreach (var step in saga.SagaSteps)
            {
                await Connection.ExecuteAsync(
                    InsertSagaStepSql,
                    new
                    {
                        Id = step.Id,
                        CorrelationId = saga.Id,
                        EventName = step.EventName,
                        State = SagaStepStateEnum.Awaiting,
                        RequestBody = step.RequestBody,
                        CreationTime = DateTime.UtcNow,
                        HasCompensation = step.HasCompensation,
                        FailedReason = step.FailedReason,
                        ExecutionSequenceNumber = step.ExecutionSequenceNumber,
                    },
                    transaction: Transaction).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public async Task<Saga> GetSagaByIdAsync(long sagaId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string QuerySagaSql =
$@"SELECT ""Id"", ""State"", ""CurrentExecutionSequenceNumber""
FROM {_sagaTableName} where ""Id"" = @Id for update;";

            string QuerySagaStepSql =
$@"SELECT ""Id"", ""CorrelationId"", ""EventName"",""HasCompensation"", ""State"", ""RequestBody"", ""FailedReason"", ""CreationTime"" ,""ExecutionSequenceNumber"" 
FROM  {_sagaStepTableName}  where ""CorrelationId"" = @CorrelationId for update;";

            var saga = await Connection.QuerySingleAsync<Saga>(
                    QuerySagaSql,
                    new { Id = sagaId }).ConfigureAwait(false);

            var sagaSteps = await Connection.QueryAsync<SagaStep>(
                    QuerySagaStepSql,
                    new { CorrelationId = sagaId }).ConfigureAwait(false);

            saga.SagaSteps = sagaSteps.ToList();

            return saga;
        }

        /// <inheritdoc/>
        public async Task UpdateSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string UpdateSagaSql =
$@"UPDATE {_sagaTableName}
SET ""State"" = @State, ""CurrentExecutionSequenceNumber"" = @CurrentExecutionSequenceNumber , ""FinishedTime"" = @FinishedTime 
WHERE ""Id"" = @Id";

            string UpdateSagaStepSql =
$@"UPDATE {_sagaStepTableName}
SET ""State"" = @State, ""FailedReason"" = @FailedReason 
WHERE ""Id"" = @Id";

            await Connection.ExecuteAsync(
                    UpdateSagaSql,
                    new { Id = saga.Id, State = saga.State, CurrentExecutionSequenceNumber = saga.CurrentExecutionSequenceNumber, FinishedTime = saga.FinishedTime },
                    transaction: Transaction).ConfigureAwait(false);

            foreach (var step in saga.SagaSteps)
            {
                await Connection.ExecuteAsync(
                    UpdateSagaStepSql,
                    new
                    {
                        Id = step.Id,
                        State = step.State,
                        FailedReason = step.FailedReason,
                    },
                    transaction: Transaction).ConfigureAwait(false);
            }
        }
    }
}
