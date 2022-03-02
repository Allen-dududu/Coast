namespace Coast.PostgreSql
{
    using System;
    using Coast.Core;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore;

    public static class CoastOptionsExtensions
    {
        public static CoastOptions UsePostgreSql(this CoastOptions options, string connectionString)
        {
            if(string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            options.RegisterExtension(serviceCollection =>
            {

                serviceCollection.AddDbContext<CoastDBContesxt>(opts =>
                    opts.UseNpgsql(connectionString));
            });

            return options;
        }
    }
}
