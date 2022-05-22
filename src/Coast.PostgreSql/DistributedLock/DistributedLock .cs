namespace Coast.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core;
    using Microsoft.Extensions.Logging;
    using Npgsql;

    public class DistributedLock : IDistributedLock
    {
        private readonly ILogger _logger;
        private bool _disposed;
        private NpgsqlConnection _connection;

        public DistributedLock(string connectionString, ILogger logger)
        {
            _logger = logger;
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            _connection = new NpgsqlConnection(builder.ToString());
            _connection.Open();
        }

        public async Task<bool> TryExecuteInDistributedLock(long lockId, Func<Task> exclusiveLockTask)
        {
            var hasLockedAcquired = await TryAcquireLockAsync(lockId).ConfigureAwait(false);

            if (!hasLockedAcquired)
            {
                return false;
            }

            try
            {
                await exclusiveLockTask().ConfigureAwait(false);
            }
            finally
            {
                await ReleaseLock(lockId).ConfigureAwait(false); ;
            }

            return true;
        }

        public async Task<bool> TryAcquireLockAsync(long lockId)
        {
            var sessionLockCommand = $"SELECT pg_try_advisory_lock({lockId})";
            _logger.LogTrace("Trying to acquire session lock for Lock Id {@LockId}", lockId);
            var commandQuery = new NpgsqlCommand(sessionLockCommand, _connection);
            var result = await commandQuery.ExecuteScalarAsync().ConfigureAwait(false);
            if (result != null && bool.TryParse(result.ToString(), out var lockAcquired) && lockAcquired)
            {
                _logger.LogTrace("Lock {@LockId} acquired", lockId);
                return true;
            }

            _logger.LogTrace("Lock {@LockId} rejected", lockId);
            return false;
        }

        private async Task ReleaseLock(long lockId)
        {
            var transactionLockCommand = $"SELECT pg_advisory_unlock({lockId})";
            _logger.LogTrace("Releasing session lock for {@LockId}", lockId);
            var commandQuery = new NpgsqlCommand(transactionLockCommand, _connection);
            await commandQuery.ExecuteScalarAsync().ConfigureAwait(false); ;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
                _connection = null;
            }

            _disposed = true;
        }
    }
}
