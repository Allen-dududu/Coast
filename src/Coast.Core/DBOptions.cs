namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class DBOptions
    {
        public const string DefaultSchema = "Coast";

        /// <summary>
        /// Gets or sets the table name prefix to use when creating database objects.
        /// </summary>
        public string TableNamePrefix { get; set; } = DefaultSchema;

        /// <summary>
        /// Data version.
        /// </summary>
        internal string Version { get; set; } = "v1";

        public string ConnectionString { get; set; }
    }
}
