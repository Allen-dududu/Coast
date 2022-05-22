namespace Coast.Core
{
    using System.Threading.Tasks;

    public interface IProcessSagaEvent
    {
        Task IdempotentProcessEvent(string eventName, SagaEvent @event);
        
        Task ProcessEvent(string eventName, SagaEvent @event);
    }
}
