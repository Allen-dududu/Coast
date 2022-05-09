namespace Coast.PostgreSql
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.MigrationManager;
    using Coast.PostgreSql.Connection;
    using Dapper;
    using Microsoft.Extensions.Options;
    using Npgsql;

    /// <summary>
    /// Init DataBase. Create table.
    /// </summary>
    public class CoastDBInitializer : ICoastDBInitializer
    {
        private readonly IConnectionProvider _connectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoastDBInitializer"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public CoastDBInitializer(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(string schema, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var sql = CreateTableSql(schema);
            using var connection = _connectionProvider.OpenConnection();
            await connection.ExecuteAsync(sql).ConfigureAwait(false);
        }

        private string CreateTableSql(string schema)
        {
            var sql = $@"
CREATE SCHEMA IF NOT EXISTS ""{schema}"";

CREATE TABLE IF NOT EXISTS ""Coast_Barrier""(
    ""Id"" bigint PRIMARY KEY NOT NULL,
    ""TransactionType"" int NOT NULL,
	""CorrelationId"" bigint NOT NULL,
	""StepId"" bigint NOT NULL,
	""StepType"" int NULL,
    ""CreationTime"" TIMESTAMP NULL
);
CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS ""Barrier_Id""
ON ""Coast_Barrier"" (""TransactionType"", ""CorrelationId"", ""StepId"", ""StepType"");

CREATE TABLE IF NOT EXISTS ""Coast_Saga""(
	""Id"" bigint PRIMARY KEY NOT NULL,
    ""State"" int NOT NULL,
    ""CurrentStep"" bigint NULL,
    ""CreationTime"" TIMESTAMP NULL
) ;

CREATE TABLE IF NOT EXISTS ""Coast_SagaStep""(
	""Id"" bigint NOT NULL,
    ""CorrelationId"" bigint NOT NULL,
    ""EventName"" VARCHAR(250) NOT NULL,
    ""State""   int NOT NULL,
    ""HasCompensation"" bool NOT NULL,
    ""RequestBody"" text NULL,
    ""FailedReason"" text NULL,
    ""CreationTime"" TIMESTAMP NULL,
    ""ExecuteOrder"" int NOT NULL,
    ""PublishedTime"" TIMESTAMP NULL
) ;
CREATE INDEX IF NOT EXISTS SagaStep_idx ON ""Coast_SagaStep"" (""CorrelationId"");


CREATE TABLE IF NOT EXISTS ""Coast_EventLog"" (
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
