namespace Coast.PostgreSql
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.MigrationManager;
    using Dapper;
    using Microsoft.Extensions.Options;
    using Npgsql;

    /// <summary>
    /// Init DataBase. Create table.
    /// </summary>
    public class CoastDBInitializer : ICoastDBInitializer
    {
        private readonly IOptions<DBOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoastDBInitializer"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public CoastDBInitializer(IOptions<DBOptions> options)
        {
            _options = options;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            var sql = CreateTableSql();
            using var connection = PostgreSqlDataConnection.OpenConnection(_options.Value.ConnectionString);
            await connection.ExecuteAsync(sql, new
            {

            });
        }

        private string CreateTableSql()
        {
            var sql = $@"
CREATE TABLE IF NOT EXISTS Coast_Barrier(
	""Id"" BIGINT PRIMARY KEY NOT NULL,
    ""TransactionType"" int NOT NULL,
	""CorrelationId"" bigint NOT NULL,
	""StepId"" VARCHAR(200) NULL,
	""StepType"" int NULL,
    ""CreateTime"" TIMESTAMP NULL,
    UNIQUE (""CorrelationId"", ""StepId"", ""StepType"")
);


";

            return sql;
        }
    }
}
