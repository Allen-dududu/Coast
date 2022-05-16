namespace Coast.PostgreSql.Connection
{
    using Coast.Core;
    using Microsoft.Extensions.Options;
    using Npgsql;
    using System.Data;

    public class ConnectionProvider : IConnectionProvider
    {
        private readonly IOptions<DBOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionProvider"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public ConnectionProvider(IOptions<DBOptions> options)
        {
            _options = options;
        }

        public IDbConnection OpenConnection()
        {
            var conn = new NpgsqlConnection(_options.Value.ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
