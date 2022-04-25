namespace Coast.Core.DataLayer
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Coast.Core.EventBus.IntegrationEventLog;

    public interface IWapperSession : IDisposable
    {
        public void StartTransaction();

        public void CommitTransaction();

        public void RollbackTransaction();

        public void Dispose();

        public ISagaRepository ConstructSagaRepository();

        public IEventLogRepository ConstructEventLogRepository();
    }
}
