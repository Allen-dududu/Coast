namespace Coast.Core
{
    using System;
    using System.Data;

    public interface IUnitOfWork : IDisposable
    {
        IDbConnection Connection { get; }

        IDbTransaction Transaction { get; }

        ISagaRepository SagaRepository { get; }

        IEventLogRepository EventLogRepository { get; }

        void Commit();
    }
}
