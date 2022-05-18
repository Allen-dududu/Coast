namespace Coast.PostgreSql.DistributedLock
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    internal class PostgresDistributedLock : IDistributedLockProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresDistributedLock> _logger;

        PostgresDistributedLock(IOptions<DBOptions> options, ILogger<PostgresDistributedLock> logger)
        {
            _connectionString = options.Value.ConnectionString;
            _logger = logger;
        }

        public IDistributedLock CreateLock(string name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return new DistributedLock(_connectionString, _logger);
        }

        public IDistributedLock CreateLock(long lockId)
        {
            throw new NotImplementedException();
        }
    }
}
