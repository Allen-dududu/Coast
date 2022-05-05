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
    using static Coast.Core.EventBus.InMemoryEventBusSubscriptionsManager;

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
                    var handler = _serviceProvider.GetService(subscription.HandlerType);
                    if (handler == null) continue;
                    dynamic eventData = JObject.Parse(message);
                    var eventType = _subsManager.GetEventTypeByName(eventName);

                    if (eventData.EventType == TransactionStepTypeEnum.Commit)
                    {
                        (object eventDataObj, Type concreteType) = ConvertEventDataAndConcreteType(eventType, message, subscription);
                        await Task.Yield();
                        var x = concreteType.GetMethod("Commit");
                        await (Task)concreteType.GetMethod("Commit").Invoke(handler, new object[] { eventDataObj });
                    }
                    else if (eventData.EventType == TransactionStepTypeEnum.Compensate)
                    {
                        (object eventDataObj, Type concreteType) = ConvertEventDataAndConcreteType(eventType, message, subscription);
                        await Task.Yield();
                        await (Task)concreteType.GetMethod("Cancel").Invoke(handler, new object[] { eventDataObj });
                    }
                    else
                    {
                        _logger.LogWarning("No Correct TransactionStepTypeEnum for saga event: {EventName}", eventName);
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for saga event: {EventName}", eventName);
            }
        }

        private (object eventDataObj, Type concreteType) ConvertEventDataAndConcreteType(Type eventType,string message, SubscriptionInfo subscription)
        {
            object eventDataObj;
            Type concreteType;
            if (subscription.IsDynamic)
            {
                eventDataObj = JsonConvert.DeserializeObject(message, typeof(SagaEvent));
                concreteType = typeof(ISagaHandler);
            }
            else
            {
                eventDataObj = JsonConvert.DeserializeObject(message, typeof(SagaEvent<>).MakeGenericType(eventType));
                concreteType = typeof(ISagaHandler<>).MakeGenericType(eventType);
            }

            return (eventDataObj, concreteType);
        }
    }
}
