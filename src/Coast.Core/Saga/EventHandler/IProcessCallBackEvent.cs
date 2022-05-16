namespace Coast.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProcessCallBackEvent
    {
        Task ProcessEventAsync(SagaEvent @event, Func<SagaEvent, CancellationToken, Task> publishEvent);
    }
}
