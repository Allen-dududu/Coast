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
(""Id"", ""State"", ""CreationTime"", ""CurrentExecutionSequenceNumber"") 
VALUES (@Id, @State, @CreationTime, @CurrentExecutionSequenceNumber ); ";

        private const string insertSagaStepSql =
@"INSERT INTO ""Coast_SagaStep"" 
(""Id"", ""CorrelationId"", ""EventName"", ""HasCompensation"", ""State"", ""RequestBody"", ""CreationTime"", ""FailedReason"", ""ExecutionSequenceNumber"") 
VALUES (@Id, @CorrelationId, @EventName, @HasCompensation, @State, @RequestBody, @CreationTime,@FailedReason, @ExecutionSequenceNumber); ";

        private const string QuerySagaSql =
@"SELECT ""Id"", ""State"", ""CurrentExecutionSequenceNumber""
FROM ""Coast_Saga"" where ""Id"" = @Id;";

        private const string QuerySagaStepSql =
@"SELECT ""Id"", ""CorrelationId"", ""EventName"",""HasCompensation"", ""State"", ""RequestBody"", ""FailedReason"", ""CreationTime"" ,""ExecutionSequenceNumber"" 
FROM ""Coast_SagaStep"" where ""CorrelationId"" = @CorrelationId;";

        private const string UpdateSagaSql =
@"UPDATE ""Coast_Saga""
SET ""State"" = @State, ""CurrentExecutionSequenceNumber"" = @CurrentExecutionSequenceNumber 
WHERE ""Id"" = @Id";

        private const string UpdateSagaStepSql =
@"UPDATE ""Coast_SagaStep""
SET ""State"" = @State, ""FailedReason"" = @FailedReason 
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
        public async Task SaveSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _connection.ExecuteAsync(
                    InsertSagaSql,
                    new { Id = saga.Id, State = SagaStateEnum.Started, CreationTime = DateTime.UtcNow, CurrentExecutionSequenceNumber = saga.CurrentExecutionSequenceNumber },
                    transaction: _transaction).ConfigureAwait(false);

            foreach (var step in saga.SagaSteps)
            {
                await _connection.ExecuteAsync(
                    insertSagaStepSql,
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
                    transaction: _transaction).ConfigureAwait(false);
            }
        }

        public async Task SaveSagaStepsAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var step in saga.SagaSteps)
            {
                await _connection.ExecuteAsync(
                    insertSagaStepSql,
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
                    new { CorrelationId = sagaId }).ConfigureAwait(false);

            saga.SagaSteps = sagaSteps.ToList();

            return saga;
        }

        /// <inheritdoc/>
        public async Task UpdateSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await _connection.ExecuteAsync(
                    UpdateSagaSql,
                    new { Id = saga.Id, State = saga.State, CurrentExecutionSequenceNumber = saga.CurrentExecutionSequenceNumber},
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
