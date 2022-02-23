using Coast.Core.EventBus;
using Microsoft.Extensions.DependencyInjection;

namespace Coast.Core
{
    public sealed class CoastBuild
    {
        public CoastBuild(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }

        public CoastBuild AddMessageHandler<T>() where T : class, IIntegrationEventHandler
        {
            Services.AddTransient<T>();
            return this;
        }
    }
}