namespace Coast.Core
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;
    using Microsoft.Extensions.Logging;
    using static Coast.Core.EventBus.InMemoryEventBusSubscriptionsManager;

    public class ProcessSagaEvent : IProcessSagaEvent
    {
        private readonly ILogger<ProcessSagaEvent> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly IBarrierService _barrierService;

        public ProcessSagaEvent(IServiceProvider serviceProvider, ILogger<ProcessSagaEvent> logger, IEventBusSubscriptionsManager subsManager, IBarrierService barrierService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _subsManager = subsManager;
            _barrierService = barrierService;
        }

        public async Task IdempotentProcessEvent(string eventName, SagaEvent @event)
        {
            // var barrier = _barrierService.CreateBranchBarrier(@event, _logger);
            // await barrier.Call(() => ProcessEvent(eventName,@event)).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task ProcessEvent(string eventName, SagaEvent @event)
        {
            _logger.LogTrace("Processing saga event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                var subscriptions = _subsManager.GetHandlersForEvent(eventName);
                foreach (var subscription in subscriptions)
                {
                    var handler = _serviceProvider.GetService(subscription.HandlerType);
                    if (handler is null) continue;

                    var eventType = _subsManager.GetEventTypeByName(eventName);
                    if (@event.StepType == TransactionStepTypeEnum.Commit)
                    {
                        (object eventDataObj, Type concreteType) = ConvertEventDataAndConcreteType(eventType, @event.RequestBody, subscription, TransactionStepTypeEnum.Commit);
                        await Task.Yield();
                        await (Task)concreteType.GetMethod("CommitAsync").Invoke(handler, new object[] { eventDataObj });
                    }
                    else if (@event.StepType == TransactionStepTypeEnum.Compensate)
                    {
                        (object eventDataObj, Type concreteType) = ConvertEventDataAndConcreteType(eventType, @event.RequestBody, subscription, TransactionStepTypeEnum.Compensate);
                        await Task.Yield();
                        await (Task)concreteType.GetMethod("CancelAsync").Invoke(handler, new object[] { eventDataObj });
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

        private (dynamic eventDataObj, Type concreteType) ConvertEventDataAndConcreteType(Type eventType, string message, SubscriptionInfo subscription, TransactionStepTypeEnum stepType)
        {
            dynamic eventDataObj;
            Type concreteType;
            if (subscription.IsDynamic)
            {
                eventDataObj = message;
                concreteType = stepType switch
                {
                    TransactionStepTypeEnum.Compensate => typeof(ICancelEventHandler),
                    TransactionStepTypeEnum.Commit => typeof(ICommitEventHandler),
                    _ => throw new NotImplementedException(),
                };
            }
            else
            {
                eventDataObj = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                concreteType = stepType switch
                {
                    TransactionStepTypeEnum.Compensate => typeof(ICancelEventHandler<>).MakeGenericType(eventType),
                    TransactionStepTypeEnum.Commit => typeof(ICommitEventHandler<>).MakeGenericType(eventType),
                    _ => throw new NotImplementedException(),
                };
            }

            return (eventDataObj, concreteType);
        }
    }
}
