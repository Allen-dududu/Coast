namespace Coast.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Coast.Core.EventBus;

    public class SagaCallBackEventHandler : IDynamicIntegrationEventHandler
    {
        public Task Handle(dynamic eventData)
        {
            throw new NotImplementedException();
        }
    }
}
