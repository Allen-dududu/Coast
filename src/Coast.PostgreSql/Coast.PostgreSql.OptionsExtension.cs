namespace Coast.PostgreSql
{
    using System;
    using Coast.Core;
    using Coast.Core.MigrationManager;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// CoastOptions Extensions for postgreSql.
    /// </summary>
    public static class CoastOptionsExtension
    {
        /// <summary>
        /// Using PostgreSql, and config connectionString of postgreSql.
        /// </summary>
        /// <param name="options">options.</param>
        /// <param name="connectionString">connectionString.</param>
        /// <returns>CoastOptions.</returns>
        /// <exception cref="ArgumentNullException">connectionString should not be empty.</exception>
        public static CoastOptions UsePostgreSql(this CoastOptions options, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            options.RegisterExtension(serviceCollection => serviceCollection.Configure<DBOptions>(db => db.ConnectionString = connectionString));
            options.RegisterExtension(serviceCollection => serviceCollection.TryAddTransient<ICoastDBInitializer, CoastDBInitializer>());

            return options;
        }
    }
}
