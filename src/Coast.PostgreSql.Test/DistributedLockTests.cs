using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Coast.PostgreSql.Test
{
    using Coast.PostgreSql;
    using Dapper;
    using Microsoft.Extensions.Logging.Abstractions;
    using Npgsql;
    using System.Threading;
    using Xunit.Abstractions;

    public class DistributedLockTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly string _connectionString;

        public DistributedLockTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _connectionString = "Host=localhost;Port=5432;database=postgres;User Id=postgres;Password=root;";
        }

        [Fact(Skip = "integration test")]
        public void DistributedLockIsAcquiredSuccessfully()
        {

            async Task ExclusiveLockTask(int node)
            {
                _testOutputHelper.WriteLine($"Executing a long running task on Node {node}");
                // Add 5 second delay
                await Task.Delay(5000);
            }

            const long lockId = 50000;

            // Simulate with 5 nodes
            var nodes = Enumerable.Range(1, 5).ToList();
            Parallel.ForEach(nodes, async node =>
            {
                // Act and Arrange
                _testOutputHelper.WriteLine($"Trying to acquire session lock and run task for Node {node}");
                using var distributedLock = new DistributedLock(_connectionString, NullLogger<DistributedLock>.Instance);
                if (!await distributedLock.TryExecuteInDistributedLock(lockId, () => ExclusiveLockTask(node)))
                {
                    _testOutputHelper.WriteLine($"Node {node} could not acquire lock");
                }
            });
        }
    }
}
