namespace Coast.Core.EventBus.TransactionBus
{
    using System.Threading;

    public interface ITransactionalBusPublishEndpoint
    {
        void Publish(IntegrationEvent @event, CancellationToken cancellationToken = default);
    }
}
