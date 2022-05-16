namespace Coast.Core
{
    public class DBOptions
    {
        public const string DefaultSchema = "Coast";

        /// <summary>
        /// Gets or sets the schema to use when creating database objects.
        /// Default is <see cref="DefaultSchema" />.
        /// </summary>
        public string Schema { get; set; } = DefaultSchema;

        /// <summary>
        /// Data version.
        /// </summary>
        internal string Version { get; set; } = "v1";

        public string ConnectionString { get; set; }
    }
}
