namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Coast.Core;
    using Coast.Core.EventBus;

    public static class ServiceCollectionExtensions
    {
        public static CoastBuild AddCosat(this IServiceCollection services, Action<CoastOptions> setupAction)
        {
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            //Options and extension service
            var options = new CoastOptions();
            setupAction.Invoke(options);

            foreach (var serviceExtension in options.Extensions)
            {
                serviceExtension(services);
            }

            return new CoastBuild(services);
        }
    }
}
