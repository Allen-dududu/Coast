namespace Coast.Core.EventBus.TransactionBus
{
    using Coast.Core.EventBus.TransactionEventBus;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;

    public class TransactionalBusPublishEndpoint : ITransactionalBusPublishEndpoint
    {
        readonly ConcurrentDictionary<Transaction, TransactionalEnlistmentNotification> _pendingActions;
        private readonly IEventBus _bus;
        private readonly ILogger<TransactionalBusPublishEndpoint> _logger;

        public TransactionalBusPublishEndpoint(IEventBus bus, ILogger<TransactionalBusPublishEndpoint> logger)
        {
            _pendingActions = new ConcurrentDictionary<Transaction, TransactionalEnlistmentNotification>();
            _bus = bus;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void Publish(IntegrationEvent @event, CancellationToken cancellationToken)
        {
            Add(() => _bus.Publish(@event, cancellationToken));
        }

        public async void Add(Action action)
        {
            if (Transaction.Current is null)
            {
                await Task.Run(() => action());
                return;
            }

            var enlistment = GetOrCreateEnlistment();

            enlistment.Add(action);

            return;
        }

        TransactionalEnlistmentNotification GetOrCreateEnlistment()
        {
            return _pendingActions.GetOrAdd(Transaction.Current, transaction =>
            {
                var transactionEnlistment = new TransactionalEnlistmentNotification(_logger);

                transaction.TransactionCompleted += TransactionCompleted;
                transaction.EnlistVolatile(transactionEnlistment, EnlistmentOptions.None);

                return transactionEnlistment;
            });
        }

        void ClearTransaction(Transaction transaction)
        {
            if (_pendingActions.TryRemove(transaction, out _))
                transaction.TransactionCompleted -= TransactionCompleted;
        }

        void TransactionCompleted(object sender, TransactionEventArgs e)
        {
            ClearTransaction(e.Transaction);
        }
    }
}
