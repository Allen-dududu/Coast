namespace Coast.Core
{
    using System.Data;
    using System.Threading.Tasks;

    public interface ISagaHandler : ICommitEventHandler, ICancelEventHandler
    {

    }

    public interface ISagaHandler<T> : ICommitEventHandler<T>, ICancelEventHandler<T> where T : IEventRequestBody
    {

    }

    public interface ICommitEventHandler<T> where T : IEventRequestBody
    {
        Task Commit(SagaEvent<T> @event);
    }

    public interface ICancelEventHandler<T> where T : IEventRequestBody
    {
        Task Cancel(SagaEvent<T> @event);
    }

    public interface ICommitEventHandler : IIntegrationEventHandler
    {
        Task Commit(SagaEvent @event);
    }

    public interface ICancelEventHandler : IIntegrationEventHandler
    {
        Task Cancel(SagaEvent @event);
    }

    public interface IIntegrationEventHandler
    {

    }

    public interface IIntegrationEventHandler<IRequestBody> where IRequestBody : IEventRequestBody
    {

    }
}
