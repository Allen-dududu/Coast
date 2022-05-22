namespace Coast.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IDistributedLock : IDisposable
    {
        Task<bool> TryExecuteInDistributedLock(long lockId, Func<Task> exclusiveLockTask);

        Task<bool> TryAcquireLockAsync(long lockId);
    }
}