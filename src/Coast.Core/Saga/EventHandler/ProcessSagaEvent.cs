namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ProcessSagaEvent : IProcessSagaEvent
    {
        private readonly ILogger<ProcessSagaEvent> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBarrierService _barrierServic;

        public ProcessSagaEvent(IServiceProvider serviceProvider, ILogger<ProcessSagaEvent> logger, IEventBusSubscriptionsManager subsManager, IBarrierService barrierServic)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _subsManager = subsManager;
            _barrierServic = barrierServic;
        }

        /// <inheritdoc/>
        public async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing saga event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    if (subscription.IsDynamic)
                    {
                        var handler = _serviceProvider.GetService(subscription.HandlerType) as IDynamicIntegrationEventHandler;
                        if (handler == null) continue;
                        dynamic eventData = JObject.Parse(message);

                        await Task.Yield();
                        await handler.Handle(eventData, null);
                    }
                    else
                    {
                        var handler = _serviceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;
                        var eventType = _subsManager.GetEventTypeByName(eventName);
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                        await Task.Yield();
                        await(Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent, null });
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for saga event: {EventName}", eventName);
            }
        }

        private async Task IdempotentProcessEvent(string eventName, string message)
        {

        }
    }
}
