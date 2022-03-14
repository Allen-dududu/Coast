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
CREATE TABLE IF NOT EXISTS Barrier(
	""Id"" BIGINT PRIMARY KEY NOT NULL,
    ""TransactionType"" VARCHAR(20) NOT NULL,
	""Name"" VARCHAR(200) NOT NULL,
	""Group"" VARCHAR(200) NULL,
	""Content"" TEXT NULL,
	""Retries"" INT NOT NULL,
	""Added"" TIMESTAMP NOT NULL,
    ""ExpiresAt"" TIMESTAMP NULL,
	""StatusName"" VARCHAR(50) NOT NULL
);


";
        }


    }
}
