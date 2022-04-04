namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Coast.Core;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class ServiceCollectionExtension
    {
        public static CoastBuild AddCosat(this IServiceCollection services, Action<CoastOptions> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.TryAddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            // Options and extension service
            var options = new CoastOptions();
            setupAction.Invoke(options);

            foreach (var serviceExtension in options.Extensions)
            {
                serviceExtension(services);
            }

            // Startup and Hosted
            services.AddSingleton<Bootstrapper>();
            services.AddHostedService(sp => sp.GetRequiredService<Bootstrapper>());

            return new CoastBuild(services);
        }
    }
}
