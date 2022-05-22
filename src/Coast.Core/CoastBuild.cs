namespace Coast.Core
{
    using Microsoft.Extensions.DependencyInjection;

    public sealed class CoastBuild
    {
        public CoastBuild(IServiceCollection services)
        {
            _Services = services;
        }

        public IServiceCollection _Services { get; }

        public CoastBuild AddMessageHandler<T>() where T : class, IEventHandler
        {
            _Services.AddTransient<T>();
            return this;
        }
    }
}