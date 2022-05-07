namespace Coast.Core.EventBus
{
    using System.Threading;

    public interface IEventBus
    {
        void Publish(IntegrationEvent @event, CancellationToken cancellationToken = default);

        void Subscribe<T, TH>()
            where T : IEventRequestBody
            where TH : ISagaHandler<T>;

        void Subscribe<TH>(string eventName)
            where TH : ISagaHandler;

        void Unsubscribe<T, TH>()
            where TH : ISagaHandler<T>
            where T : IEventRequestBody;

        void Unsubscribe<TH>(string eventName)
            where TH : ISagaHandler;
    }
}
