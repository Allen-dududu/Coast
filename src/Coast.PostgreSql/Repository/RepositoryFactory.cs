namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.PostgreSql.Connection;

    internal class RepositoryFactory : IRepositoryFactory, IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        private readonly IConnectionProvider _connectionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryFactory"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public RepositoryFactory(IConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public IWapperSession OpenSession(IDbConnection dbConnection = null)
        {
            var connection = dbConnection ?? _connection;
           return new WrapperSession(connection);
        }

        public void Dispose()
        {
            _connection?.Close();
        }
    }
}
