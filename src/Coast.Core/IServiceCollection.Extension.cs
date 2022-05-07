namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Coast.Core;
    using Coast.Core.EventBus;
    using Coast.Core.Util;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class SetUpExtension
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
            if (string.IsNullOrWhiteSpace(options.DomainName))
            {
                throw new InvalidOperationException("You must be config DomainName For Coast!");
            }

            Const.DomainName = options.DomainName;
            Const.WorkerId = options.WorkerId;
            services.AddSingleton<CoastOptions>(options);

            foreach (var serviceExtension in options.Extensions)
            {
                serviceExtension(services);
            }

            services.TryAddTransient<IBarrierService, DefaultBarrierService>();
            services.TryAddTransient<SagaCallBackEventHandler>();
            services.TryAddTransient<ISagaManager, SagaManager>();
            services.TryAddTransient<IProcessSagaEvent, ProcessSagaEvent>();

            // Startup and Hosted
            services.AddSingleton<Bootstrapper>();
            services.AddHostedService(sp => sp.GetRequiredService<Bootstrapper>());

            return new CoastBuild(services);
        }
    }
}
