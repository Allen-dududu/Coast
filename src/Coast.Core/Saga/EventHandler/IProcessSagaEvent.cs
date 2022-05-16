namespace Coast.Core
{
    using System.Threading.Tasks;

    public interface IProcessSagaEvent
    {
        Task ProcessEvent(string eventName, SagaEvent @event);
    }
}
