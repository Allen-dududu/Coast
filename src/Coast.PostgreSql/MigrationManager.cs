namespace Coast.PostgreSql
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class MigrationManager
    {
        //public static IHost MigrateCosat(this IHost host)
        //{
        //    using (var scope = host.Services.CreateScope())
        //    {
        //        using (var appContext = scope.ServiceProvider.GetRequiredService<>())
        //        {
        //            try
        //            {
        //                appContext.Database.Migrate();
        //            }
        //            catch (Exception)
        //            {
        //                // Log errors or do anything you think it's needed
        //                throw;
        //            }
        //        }
        //    }

        //    return host;
        ////}
    }
}
