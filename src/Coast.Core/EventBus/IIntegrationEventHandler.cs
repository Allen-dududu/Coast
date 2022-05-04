namespace Coast.Core.EventBus
{
    using System.Data;
    using System.Threading.Tasks;

    public interface IIntegrationEventHandler<IRequestBody> : IIntegrationEventHandler where IRequestBody : IEventRequestBody
    {
        Task Commit(SagaEvent<IRequestBody> @event, IDbTransaction transaction = null);
    }

    public interface ISagaHandler : ICommitEventHandler, ICancelEventHandler
    {

    }

    public interface ISagaHandler<IRequestBody> : ICommitEventHandler, ICancelEventHandler
    {

    }

    public interface ICommitEventHandler<IRequestBody> : IIntegrationEventHandler where IRequestBody : IEventRequestBody
    {
        Task Commit(SagaEvent<IRequestBody> @event);
    }

    public interface ICancelEventHandler<IRequestBody> : IIntegrationEventHandler where IRequestBody : IEventRequestBody
    {
        Task Cancel(SagaEvent<IRequestBody> @event);
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
}
