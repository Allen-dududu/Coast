namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;

    public class CoastOptions
    {
        /// <summary>
        /// Gets or sets if failed, how many count should retry.
        /// </summary>
        public int FailedRetryCount { get; set; }

        internal IList<Action<IServiceCollection>> Extensions { get; }

        public CoastOptions()
        {
            FailedRetryCount = 50;
        }

        /// <summary>
        /// Registers an extension that will be executed when building services.
        /// </summary>
        /// <param name="extension">action of IServiceCollection.</param>
        public void RegisterExtension(Action<IServiceCollection> extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);
        }
    }
}
