using System;
using System.Collections.Generic;
using System.Text;

namespace Coast.Core
{
    public class CoastOptions
    {
        public CoastOptions()
        {
            FailedRetryCount = 50;
        }

        public int FailedRetryCount = 50;

        internal IList<ICoastOptionsExtension> Extensions { get; }

        /// <summary>
        /// Registers an extension that will be executed when building services.
        /// </summary>
        /// <param name="extension"></param>
        public void RegisterExtension(ICoastOptionsExtension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);
        }
    }
}
