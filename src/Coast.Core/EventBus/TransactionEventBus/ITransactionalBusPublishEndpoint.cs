namespace Coast.Core.EventBus.TransactionBus
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ITransactionalBusPublishEndpoint
    {
        void Publish(IntegrationEvent @event, CancellationToken cancellationToken = default);
    }
}
