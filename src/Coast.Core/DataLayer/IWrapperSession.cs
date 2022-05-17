namespace Coast.Core.DataLayer
{
    using System;
    using System.Data;
    using Coast.Core.EventBus.EventLog;

    public interface IWapperSession : IDisposable
    {
        public IDbTransaction StartTransaction(IDbTransaction transaction = null);

        public void CommitTransaction();

        public void RollbackTransaction();

        public void Dispose();

        public ISagaRepository ConstructSagaRepository();

        public IEventLogRepository ConstructEventLogRepository();
    }
}
