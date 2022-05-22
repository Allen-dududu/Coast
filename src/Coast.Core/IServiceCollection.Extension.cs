namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using Coast.Core;
    using Coast.Core.EventBus;
    using Coast.Core.Processor;
    using Coast.Core.Util;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    public static class SetUpExtension
    {
        public static CoastBuild AddCosat(this IServiceCollection services, Action<CoastOptions> setupAction)
        {
            if (setupAction is null)
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

            CoastConstant.DomainName = options.DomainName;
            CoastConstant.WorkerId = options.WorkerId;
            services.AddSingleton<CoastOptions>(options);
            services.TryAddTransient<IBarrierService, DefaultBarrierService>();
            services.TryAddTransient<SagaCallBackEventHandler>();
            services.TryAddTransient<ISagaManager, SagaManager>();
            services.TryAddTransient<IProcessSagaEvent, ProcessSagaEvent>();

            foreach (var serviceExtension in options.Extensions)
            {
                serviceExtension(services);
            }

            // Processors
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IProcessingServer, CoastProcessingServer>());

            // Queue's message processor
            services.TryAddSingleton<MessageNeedToRetryProcessor>();

            // Startup and Hosted
            services.AddSingleton<Bootstrapper>();
            services.AddHostedService(sp => sp.GetRequiredService<Bootstrapper>());

            return new CoastBuild(services);
        }
    }
}
