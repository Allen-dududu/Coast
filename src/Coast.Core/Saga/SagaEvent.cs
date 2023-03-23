namespace Coast.Core
{
    using Coast.Core.EventBus;

    public class SagaEvent : IntegrationEvent
    {
        public string RequestBody { get; set; }

        public string ErrorMessage { get; set; }

        public bool NotAllowedFail
        {
            get; internal set;
        }
    }

    public class SagaEvent<T> : IntegrationEvent where T : EventRequestBody
    {
        public T RequestBody { get; set; }

        public string ErrorMessage { get; set; }
        public bool NotAllowedFail
        {
            get; internal set;
        }
    }
}
