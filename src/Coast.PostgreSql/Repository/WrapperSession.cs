namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus.EventLog;
    using Coast.PostgreSql.Service;

    public class WrapperSession : IWapperSession, IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private readonly string _schemaName;

        public WrapperSession(IDbConnection dbConnection, string schemaName)
        {
            _connection = dbConnection ?? throw new Exception("DB connection cannot be null");
            _schemaName = schemaName;
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
        }

        public IDbTransaction StartTransaction(IDbTransaction transaction)
        {
            _transaction = transaction ?? _connection.BeginTransaction();
            return _transaction;
        }

        public void CommitTransaction()
        {
            _transaction?.Commit();
            _connection?.Close();
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
            _connection?.Close();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _transaction?.Commit();
            _connection?.Close();
        }

        public ISagaRepository ConstructSagaRepository()
        {
            return new SagaRepository(_schemaName, _connection, _transaction);
        }

        public IEventLogRepository ConstructEventLogRepository()
        {
            return new EventLogRepository(_schemaName, _connection, _transaction);
        }
    }
}
