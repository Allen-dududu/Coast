namespace Coast.Core
{
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;

    public class CoastOptions
    {
        /// <summary>
        /// Gets or sets if failed, how many count should retry.
        /// </summary>
        public int FailedRetryCount { get; set; }

        public string DomainName { get; set; }

        public string? WorkerId { get; set; }

        internal IList<Action<IServiceCollection>> Extensions { get; } = new List<Action<IServiceCollection>>();

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
            if (extension is null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);
        }
    }
}
