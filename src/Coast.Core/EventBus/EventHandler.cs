namespace Coast.Core
{
    using System.Data;
    using System.Threading.Tasks;

    public interface ISagaHandler : ICommitEventHandler, ICancelEventHandler
    {

    }

    public interface ISagaHandler<T> : ICommitEventHandler<T>, ICancelEventHandler<T> where T : EventRequestBody
    {

    }

    public interface ICommitEventHandler<T> where T : EventRequestBody
    {
        public Task CommitAsync(T @event);
    }

    public interface ICancelEventHandler<T> where T : EventRequestBody
    {
        public Task CancelAsync(T @event);
    }

    public interface ICommitEventHandler : EventHandler
    {
        public Task CommitAsync(string @event);
    }

    public interface ICancelEventHandler : EventHandler
    {
        public Task CancelAsync(string @event);
    }

    public interface EventHandler
    {

    }

    public interface IIntegrationEventHandler<IRequestBody> where IRequestBody : EventRequestBody
    {

    }
}
