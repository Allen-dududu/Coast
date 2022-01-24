using Coast.Core;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static CoastBuild AddCosat(this IServiceCollection services, Action<CoastOptions> setupAction)
        {
            services.AddSingleton()
        }
    }
}
