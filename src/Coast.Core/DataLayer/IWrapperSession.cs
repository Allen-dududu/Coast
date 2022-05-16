namespace Coast.Core.DataLayer
{
    using Coast.Core.EventBus.EventLog;
    using System;
    using System.Data;

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
