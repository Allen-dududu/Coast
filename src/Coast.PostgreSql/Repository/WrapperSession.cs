namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.Core.EventBus.IntegrationEventLog;
    using Coast.PostgreSql.Service;

    public class WrapperSession : IWapperSession, IDisposable
    {
        private IDbConnection _connection;
        private IDbTransaction _transaction;

        public WrapperSession(IDbConnection dbConnection)
        {
            _connection = dbConnection ?? throw new Exception("DB connection cannot be null");
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
        }

        public void RollbackTransaction()
        {
            _transaction?.Rollback();
        }

        public void Dispose()
        {
            _connection?.Close();
        }

        public ISagaRepository ConstructSagaRepository()
        {
            return new SagaRepository(_connection, _transaction);
        }

        public IEventLogRepository ConstructEventLogRepository()
        {
            return new EventLogRepository(_connection, _transaction);
        }
    }
}
