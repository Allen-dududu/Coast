namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.PostgreSql.Connection;
    using Dapper;

    public class SagaRepository : ISagaRepository
    {
        private const string InsertSagaSql =
@"INSERT INTO ""Coast_Saga"" 
(""Id"", ""State"", ""CreationTime"", ""CurrentStep"") 
VALUES (@Id, @State, @CreationTime, @CurrentStep ); ";

        private const string insertSagaStepSql =
@"INSERT INTO ""Coast_SagaStep"" 
(""Id"", ""CorrelationId"", ""EventName"", ""HasCompensation"", ""State"", ""RequestBody"", ""CreationTime"", ""FailedReason"", ""ExecuteOrder"") 
VALUES (@Id, @CorrelationId, @EventName, @HasCompensation, @State, @RequestBody, @CreationTime,@FailedReason, @ExecuteOrder); ";

        private const string QuerySagaSql =
@"SELECT ""Id"", ""State"", ""CurrentStep""
FROM ""Coast_Saga"" where ""Id"" = @SagaId;";

        private const string QuerySagaStepSql =
@"SELECT ""Id"", ""CorrelationId"", ""EventName"",""HasCompensation"", ""State"", ""RequestBody"", ""FailedReason"", ""CreationTime"" ,""ExecuteOrder"" 
FROM ""Coast_SagaStep"" where ""CorrelationId"" = @SagaId;";

        private const string UpdateSagaSql =
@"UPDATE ""Coast_Saga""
SET ""State"" = @State, ""CurrentStep"" = @CurrentStep 
WHERE ""Id"" = @Id";

        private const string UpdateSagaStepSql =
@"UPDATE ""Coast_SagaStep""
SET ""State"" = @State, ""FailedReason"" = @FailedReason,  
WHERE ""Id"" = @Id";

        private IDbConnection _connection;
        private IDbTransaction _transaction;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaRepository"/> class.
        /// </summary>
        public SagaRepository(IDbConnection connection, IDbTransaction transaction = null)
        {
            _connection = connection;
            _transaction = transaction;
        }

        /// <inheritdoc/>
        public async Task AddSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sagaId = SnowflakeId.Default().NextId();
            await _connection.ExecuteAsync(
                    InsertSagaSql,
                    new { Id = sagaId, State = SagaStateEnum.Started, CreationTime = DateTime.UtcNow, CurrentStep = saga.CurrenExecuteOrder },
                    transaction: _transaction).ConfigureAwait(false);

            foreach (var step in saga.SagaSteps)
            {
                await _connection.ExecuteAsync(
                    insertSagaStepSql,
                    new
                    {
                        Id = SnowflakeId.Default().NextId(),
                        CorrelationId = sagaId,
                        EventName = step.EventName,
                        State = SagaStepStateEnum.Awaiting,
                        RequestBody = step.RequestBody,
                        CreationTime = DateTime.UtcNow,
                        HasCompensation = step.HasCompensation,
                        FailedReason = step.FailedReason,
                        ExecuteOrder = step.ExecuteOrder,
                    },
                    transaction: _transaction).ConfigureAwait(false);
            }

        }

        /// <inheritdoc/>
        public async Task<Saga> GetSagaByIdAsync(long sagaId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var saga = await _connection.QuerySingleAsync<Saga>(
                    QuerySagaSql,
                    new { Id = sagaId}).ConfigureAwait(false);

            var sagaSteps = await _connection.QueryAsync<SagaStep>(
                    QuerySagaStepSql,
                    new { Id = sagaId }).ConfigureAwait(false);

            saga.SagaSteps = sagaSteps.ToList();

            return saga;
        }

        /// <inheritdoc/>
        public async Task UpdateSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _connection.ExecuteAsync(
                    UpdateSagaSql,
                    new { Id = saga.Id, State = saga.State, CurrentStep = saga.CurrenExecuteOrder},
                    transaction: _transaction).ConfigureAwait(false);

            foreach (var step in saga.SagaSteps)
            {
                await _connection.ExecuteAsync(
                    UpdateSagaStepSql,
                    new
                    {
                        Id = step.Id,
                        State = step.State,
                        FailedReason = step.FailedReason,
                    },
                    transaction: _transaction).ConfigureAwait(false);
            }
        }
    }
}
