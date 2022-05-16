namespace Coast.PostgreSql.Repository
{
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Microsoft.Extensions.Options;
    using Npgsql;
    using System;
    using System.Data;

    internal class RepositoryFactory : IRepositoryFactory, IDisposable
    {
        private IDbConnection _connection;

        private readonly IOptions<DBOptions> _options;

        public RepositoryFactory(IOptions<DBOptions> options)
        {
            this._options = options;
        }

        public IWapperSession OpenSession(IDbConnection dbConnection = null)
        {
            _connection = dbConnection ?? OpenConnection();
            return new WrapperSession(_connection, _options.Value.Schema);
        }

        public IDbConnection OpenConnection()
        {
            var conn = new NpgsqlConnection(_options.Value.ConnectionString);
            conn.Open();
            return conn;
        }

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}
