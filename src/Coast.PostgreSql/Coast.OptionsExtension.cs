namespace Coast.PostgreSql
{
    using System;
    using System.Data.Common;
    using Coast.Core;
    using Coast.Core.DataLayer;
    using Coast.Core.Idempotent;
    using Coast.Core.MigrationManager;
    using Coast.PostgreSql.Connection;
    using Coast.PostgreSql.Repository;
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
            options.RegisterExtension(ServiceCollection => ServiceCollection.TryAddTransient<IConnectionProvider, ConnectionProvider>());
            options.RegisterExtension(serviceCollection => serviceCollection.TryAddTransient<ICoastDBInitializer, CoastDBInitializer>());
            options.RegisterExtension(ServiceCollection => ServiceCollection.TryAddTransient<IRepositoryFactory, RepositoryFactory>());
            options.RegisterExtension(ServiceCollection => ServiceCollection.TryAddTransient<IBranchBarrierRepository, BranchBarrierRepository>());

            

            return options;
        }
    }
}
