namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.EventBus;
    using Microsoft.AspNetCore.Builder;

    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder CoastSubscribe<T, TH>(this IApplicationBuilder app) 
            where T : IEventRequestBody
            where TH : ISagaHandler<T>
        {
            var evebtBus = (IEventBus)app.ApplicationServices.GetService(typeof(IEventBus));
            evebtBus.Subscribe<T, TH>();
            return app;
        }

        public static IApplicationBuilder CoastSubscribe<TH>(this IApplicationBuilder app, string eventName) where TH : ISagaHandler
        {
            var evebtBus = (IEventBus)app.ApplicationServices.GetService(typeof(IEventBus));
            evebtBus.Subscribe<TH>(eventName);
            return app;
        }
    }
}
