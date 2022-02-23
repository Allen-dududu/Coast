using Coast.Core;
using Coast.Core.EventBus;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static CoastBuild AddCosat(this IServiceCollection services, Action<CoastOptions> setupAction)
        {
            if(setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();


            //Options and extension service
            var options = new CoastOptions();
            setupAction.Invoke(options);
            
            return new CoastBuild(services);
        }
    }
}
