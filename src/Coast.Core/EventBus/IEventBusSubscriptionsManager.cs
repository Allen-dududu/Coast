namespace Coast.Core.EventBus
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using static Coast.Core.EventBus.InMemoryEventBusSubscriptionsManager;

    public interface IEventBusSubscriptionsManager
    {
        bool IsEmpty { get; }

        event EventHandler<string> OnEventRemoved;

        void AddSubscription<T, TH>()
           where T : IEventRequestBody
           where TH : ISagaHandler<T>;

        void RemoveSubscription<T, TH>()
             where TH : ISagaHandler<T>
             where T : IEventRequestBody;

        void AddSubscription<TH>(string eventName)
            where TH : ISagaHandler;

        void RemoveSubscription<TH>(string eventName)
            where TH : ISagaHandler;

        bool HasSubscriptionsForEvent<T>() where T : IEventRequestBody;

        bool HasSubscriptionsForEvent(string eventName);

        Type GetEventTypeByName(string eventName);

        void Clear();

        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IEventRequestBody;

        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);

        string GetEventKey<T>();
    }
}
