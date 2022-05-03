namespace Coast.Core.EventBus
{
    using System.Data;
    using System.Threading.Tasks;

    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event, IDbTransaction transaction = null);
    }

    public interface IIntegrationEventHandler
    {
    }
}
