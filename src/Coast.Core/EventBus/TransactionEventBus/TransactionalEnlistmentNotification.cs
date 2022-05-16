namespace Coast.Core.EventBus.TransactionEventBus
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Transactions;

    public class TransactionalEnlistmentNotification :
        IEnlistmentNotification
    {
        readonly List<Action> _pendingActions;

        private readonly ILogger _logger;

        public TransactionalEnlistmentNotification(ILogger logger)
        {
            _pendingActions = new List<Action>();
            this._logger = logger;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            _logger.LogDebug("Prepare notification received");

            try
            {
                ExecutePendingActions();

                //If work finished correctly, reply prepared
                preparingEnlistment.Prepared();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "MassTransit: Error executing pending actions");

                preparingEnlistment.ForceRollback();
            }
        }

        public void Commit(Enlistment enlistment)
        {
            _logger.LogDebug("Commit notification received");

            DiscardPendingActions();

            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            _logger.LogDebug("Rollback notification received");

            DiscardPendingActions();

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            _logger.LogDebug("In doubt notification received");

            DiscardPendingActions();

            enlistment.Done();
        }

        public void Add(Action method)
        {
            lock (_pendingActions)
                _pendingActions.Add(method);
        }

        void ExecutePendingActions()
        {
            Action[] pendingActions;
            lock (_pendingActions)
                pendingActions = _pendingActions.ToArray();

            foreach (Action action in pendingActions)
                action();
        }

        void DiscardPendingActions()
        {
            lock (_pendingActions)
                _pendingActions.Clear();
        }
    }
}
