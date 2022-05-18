namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Acts as a factory for <see cref="IDistributedLock"/> instances of a certain type. This interface may be
    /// easier to use than <see cref="IDistributedLock"/> in dependency injection scenarios.
    /// </summary>
    public interface IDistributedLockProvider
    {
        /// <summary>
        /// Constructs an <see cref="IDistributedLock"/> instance with the given <paramref name="name"/>.
        /// </summary>
        IDistributedLock CreateLock(string name);

        /// <summary>
        /// Constructs an <see cref="IDistributedLock"/> instance with the given <paramref lockId="lockId"/>.
        /// </summary>
        IDistributedLock CreateLock(long lockId);
    }
}
