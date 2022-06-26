namespace Coast.PostgreSql
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class PostgresDistributedLock : IDistributedLockProvider
    {
        private readonly string _connectionString;
        private readonly ILogger<PostgresDistributedLock> _logger;

        public PostgresDistributedLock(IOptions<DBOptions> options, ILogger<PostgresDistributedLock> logger)
        {
            _connectionString = options.Value.ConnectionString;
            _logger = logger;
        }

        public IDistributedLock CreateLock()
        {
            return new DistributedLock(_connectionString, _logger);
        }
    }
}
