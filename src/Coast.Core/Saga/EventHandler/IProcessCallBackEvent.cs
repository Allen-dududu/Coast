namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public interface IProcessCallBackEvent
    {
        Task ProcessEventAsync(SagaEvent @event);
    }
}
