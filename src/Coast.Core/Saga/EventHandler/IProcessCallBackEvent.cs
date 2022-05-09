namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProcessCallBackEvent
    {
        Task ProcessEventAsync(SagaEvent @event, Func<SagaEvent, CancellationToken, Task> publishEvent);
    }
}
