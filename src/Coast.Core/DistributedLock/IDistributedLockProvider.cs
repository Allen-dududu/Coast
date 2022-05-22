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
        /// Constructs an <see cref="IDistributedLock"/>.
        /// </summary>
        IDistributedLock CreateLock();
    }
}
