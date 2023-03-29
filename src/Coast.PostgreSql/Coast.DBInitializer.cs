namespace Coast.PostgreSql
{
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.MigrationManager;
    using Dapper;

    /// <summary>
    /// Init DataBase. Create table.
    /// </summary>
    public class CoastDBInitializer : ICoastDBInitializer
    {
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoastDBInitializer"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public CoastDBInitializer(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(string schema, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var sql = CreateTableSql(schema);
            using var connection = _unitOfWork.Connection;
            var r = await connection.ExecuteAsync(sql).ConfigureAwait(false);
            _unitOfWork.Commit();
        }

        private string CreateTableSql(string schema)
        {
            var sql = $@"
CREATE SCHEMA IF NOT EXISTS ""{schema}""; 

CREATE TABLE IF NOT EXISTS ""{schema}"".""Barrier""( 
    ""Id"" bigint PRIMARY KEY NOT NULL,
    ""TransactionType"" int NOT NULL,
	""CorrelationId"" bigint NOT NULL,
	""StepId"" bigint NOT NULL,
	""StepType"" int NULL,
    ""IsCallBack"" bool NOT NULL,
    ""CreationTime"" TIMESTAMP NULL,
    CONSTRAINT barrier_unqiue UNIQUE (""TransactionType"", ""CorrelationId"", ""StepId"", ""StepType"", ""IsCallBack"")
);

CREATE TABLE IF NOT EXISTS ""{schema}"".""Saga""(
	""Id"" bigint PRIMARY KEY NOT NULL,
    ""State"" int NOT NULL,
    ""CurrentExecutionSequenceNumber"" bigint NULL,
    ""CreationTime"" TIMESTAMP NULL,
    ""FinishedTime"" TIMESTAMP NULL
) ;

CREATE TABLE IF NOT EXISTS ""{schema}"".""SagaStep""( 
	""Id"" bigint NOT NULL,
    ""CorrelationId"" bigint NOT NULL,
    ""EventName"" VARCHAR(250) NOT NULL,
    ""State""   int NOT NULL,
    ""HasCompensation"" bool NOT NULL,
    ""RequestBody"" text NULL,
    ""FailedReason"" text NULL,
    ""CreationTime"" TIMESTAMP NULL,
    ""ExecutionSequenceNumber"" int NOT NULL,
    ""PublishedTime"" TIMESTAMP NULL,
    CONSTRAINT SagaStep_unqiueKey UNIQUE (""CorrelationId"", ""Id"")

) ;

CREATE TABLE IF NOT EXISTS ""{schema}"".""EventLog"" (
    ""EventId"" bigint NOT NULL,
    ""CreationTime"" TIMESTAMP NULL,
    ""EventTypeName"" VARCHAR(250) NOT NULL,
    ""Content"" text NULL,
    ""State"" int NOT NULL,
    ""TimesSent"" int NOT NULL
)"
;
            return sql;
        }
    }
}
