namespace Coast.PostgreSql.Repository
{
    using System;
    using System.Data;
    using System.Threading.Tasks;
    using Coast.Core;
    using Microsoft.Extensions.Options;
    using Npgsql;

    internal class UnitOfWork : IUnitOfWork
    {
        private readonly string _schemaName;

        private IDbConnection _connection;
        private IDbTransaction _transaction;
        private bool _disposed;
        private ISagaRepository _sagaRepository;
        private IEventLogRepository _eventLogRepository;

        public IDbConnection Connection => _connection ?? throw new InvalidOperationException("Connection has been Closed.");

        public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("Transaction is null.");

        public ISagaRepository SagaRepository
        {
            get { return _sagaRepository ?? (_sagaRepository = new SagaRepository(_schemaName, _transaction)); }
        }

        public IEventLogRepository EventLogRepository
        {
            get { return _eventLogRepository ?? (_eventLogRepository = new EventLogRepository(_schemaName, _transaction)); }
        }

        public UnitOfWork(IOptions<DBOptions> options)
        {
            _schemaName = options.Value.Schema;
            _connection = new NpgsqlConnection(options.Value.ConnectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();
        }

        public void Commit()
        {
            try
            {
                _transaction.Commit();
            }
            catch
            {
                _transaction.Rollback();
                throw;
            }
            finally
            {
                _transaction.Dispose();
                _transaction = _connection.BeginTransaction();
                resetRepositories();
            }
        }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void resetRepositories()
        {
            _sagaRepository = null;
            _eventLogRepository = null;
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_transaction != null)
                    {
                        _transaction.Dispose();
                        _transaction = null;
                    }

                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }

                _disposed = true;
            }
        }

        ~UnitOfWork()
        {
            dispose(false);
        }
    }
}
