namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IProcessSagaEvent
    {
        Task ProcessEvent(string eventName, SagaEvent @event);
    }
}
