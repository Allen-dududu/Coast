namespace Coast.Core
{
    using System.Collections.Generic;
    using Coast.Core.EventBus;

    public class SagaEvent : IntegrationEvent
    {
        public string RequestBody { get; set; }

        public string ErrorMessage { get; set; }

    }

    public class SagaEvent<T> : IntegrationEvent where T : EventRequestBody
    {
        public T RequestBody { get; set; }

        public string ErrorMessage { get; set; }
    }
}
