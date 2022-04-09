namespace Coast.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.Saga;
    using Dapper;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class SagaRepository : ISagaRepository
    {
        private readonly IOptions<DBOptions> _options;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SagaRepository"/> class.
        /// </summary>
        /// <param name="options">Db configuration.</param>
        public SagaRepository(IOptions<DBOptions> options, ILogger<SagaRepository> logger)
        {
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task AddSagaAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

//            var insertSagaSql =
//@"INSERT INTO ""Coast_Saga"" 
//(""Id"", ""Status"", ""CreateTime"") 
//VALUES (@Id, @Status, @CreateTime); ";

//            var insertSagaStepSql =
//@"INSERT INTO ""Coast_Saga"" 
//(""Id"", ""CorrelationId"", ""EventName"", ""StepType"", ""Status"", ""RequestBody"", ""CreateTime"")
//VALUES (@Id, @CorrelationId, @EventName, @StepType, @Status, @RequestBody, @CreateTime); ";

//            using var connection = PostgreSqlDataConnection.OpenConnection(_options.Value.ConnectionString);
//            using var tran = connection.BeginTransaction();

//            try
//            {
//                var sagaId = SnowflakeId.Default().NextId();
//                await connection.ExecuteAsync(
//                        insertSagaSql,
//                        new { Id = sagaId, Status = SagaStatusEnum.Started, CreateTime = DateTime.UtcNow }).ConfigureAwait(false);

//                foreach (var step in saga.SagaSteps)
//                {
//                    var sagaStepId = SnowflakeId.Default().NextId();
//                    var s1 = step.Item1;
//                    var s2 = step.Item2;
//                    await connection.ExecuteAsync(
//                        insertSagaStepSql,
//                        new { Id = sagaStepId, CorrelationId = sagaId, EventName = s1.EventName, StepType = SagaStepTypeEnum.Commit, Status = SagaStepStatusEnum.Awaiting, RequestBody = s1.RequestBody, CreateTime = DateTime.UtcNow }).ConfigureAwait(false);

//                    if (s2 != null)
//                    {
//                        await connection.ExecuteAsync(
//                       insertSagaStepSql,
//                       new { Id = sagaStepId, CorrelationId = sagaId, EventName = s2.EventName, StepType = SagaStepTypeEnum.Compensate, Status = SagaStepStatusEnum.Awaiting, RequestBody = s2.RequestBody, CreateTime = DateTime.UtcNow }).ConfigureAwait(false);
//                    }
//                }

//                tran.Commit();
//            }
//            catch (Exception ex)
//            {
//                tran.Rollback();
//                _logger.LogError(ex, "-----error of insert saga into the postgreSql DB-----");

//                throw;
//            }
        }

        public Task<Saga> GetSagaByIdAsync(long sagaId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateSagaByIdAsync(Saga saga, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            throw new NotImplementedException();
        }
    }
}
