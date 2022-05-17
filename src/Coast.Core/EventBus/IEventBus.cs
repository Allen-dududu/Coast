namespace Coast.Core.EventBus
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IEventBus
    {
        void Publish(IntegrationEvent @event, CancellationToken cancellationToken = default);

        Task PublishWithLogAsync(IntegrationEvent @event, CancellationToken cancellationToken = default);

        void Subscribe<T, TH>()
            where T : EventRequestBody
            where TH : ISagaHandler<T>;

        void Subscribe<TH>(string eventName)
            where TH : ISagaHandler;

        void Unsubscribe<T, TH>()
            where TH : ISagaHandler<T>
            where T : EventRequestBody;

        void Unsubscribe<TH>(string eventName)
            where TH : ISagaHandler;
    }
}
