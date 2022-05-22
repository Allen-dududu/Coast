namespace Coast.Core
{
    using System.Threading.Tasks;

    public interface ISagaHandler : ICommitEventHandler, ICancelEventHandler
    {

    }

    public interface ISagaHandler<T> : ICommitEventHandler<T>, ICancelEventHandler<T> where T : EventRequestBody
    {

    }

    public interface ICommitEventHandler<in T> where T : EventRequestBody
    {
        public Task CommitAsync(T @event);
    }

    public interface ICancelEventHandler<in T> where T : EventRequestBody
    {
        public Task CancelAsync(T @event);
    }

    public interface ICommitEventHandler : IEventHandler
    {
        public Task CommitAsync(string @event);
    }

    public interface ICancelEventHandler : IEventHandler
    {
        public Task CancelAsync(string @event);
    }

    public interface IEventHandler
    {

    }
    public interface IIntegrationEventHandler<IRequestBody> where IRequestBody : EventRequestBody
    {

    }
}
